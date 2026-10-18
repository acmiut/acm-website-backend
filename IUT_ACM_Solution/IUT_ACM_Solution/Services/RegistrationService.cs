using IUT_ACM_Solution.Data;
using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace IUT_ACM_Solution.Services
{
    /// <summary>
    /// Two-step sign-up:
    ///   1. StartRegistrationAsync  -> data goes to PendingRegistrations (not to AspNetUsers) + email with a link.
    ///   2. ConfirmEmailAsync       -> link used: the real User is created and the pending row is deleted.
    /// A person who never clicks the link never becomes a User.
    /// </summary>
    public class RegistrationService : IRegistrationService
    {
        public const string UniversityName = "دانشگاه صنعتی اصفهان";
        private const string DefaultRole = "User";

        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);
        private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IEmailService emailService;
        private readonly IConfiguration configuration;
        private readonly ILogger<RegistrationService> logger;

        public RegistrationService(
            ApplicationDbContext db,
            UserManager<User> userManager,
            IPasswordHasher<User> passwordHasher,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<RegistrationService> logger)
        {
            this.db = db;
            this.userManager = userManager;
            this.passwordHasher = passwordHasher;
            this.emailService = emailService;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<RegistrationOutcome> StartRegistrationAsync(RegisterRequestDto dto, CancellationToken ct = default)
        {
            var email = NormalizeEmail(dto.Email);
            var now = DateTime.UtcNow;

            await PurgeExpiredAsync(now, ct);

            if (await userManager.FindByEmailAsync(email) != null)
                return RegistrationOutcome.Fail(RegistrationStatus.EmailAlreadyRegistered);

            // Not a real user: only used as the "owner" argument of the hasher and the password validators.
            var probe = new User { UserName = email, Email = email };

            if (!await IsPasswordValidAsync(probe, dto.Password))
                return RegistrationOutcome.Fail(RegistrationStatus.WeakPassword);

            var pending = await db.PendingRegistrations.FirstOrDefaultAsync(p => p.Email == email, ct);
            if (pending != null)
            {
                var wait = RemainingCooldownSeconds(pending, now);
                if (wait > 0) return RegistrationOutcome.Wait(wait);
            }

            // Same address submitted again (typo fixed, mail lost...): overwrite the old pending row.
            var isNew = pending == null;
            pending ??= new PendingRegistration { Id = Guid.NewGuid(), Email = email, CreatedAt = now };

            pending.FirstName = dto.FirstName.Trim();
            pending.LastName = dto.LastName.Trim();
            pending.ContactNumber = dto.ContactNumber.Trim();
            pending.StudentNumber = dto.StudentNumber.Trim();
            pending.EntryYear = dto.EntryYear;
            pending.EducationLevel = dto.EducationLevel;
            pending.TelegramId = NormalizeTelegramId(dto.TelegramId);
            pending.PasswordHash = passwordHasher.HashPassword(probe, dto.Password);

            return await IssueTokenAndSendAsync(pending, isNew, ct);
        }

        public async Task<RegistrationOutcome> ResendConfirmationAsync(string email, CancellationToken ct = default)
        {
            var normalized = NormalizeEmail(email);
            var now = DateTime.UtcNow;

            var pending = await db.PendingRegistrations.FirstOrDefaultAsync(p => p.Email == normalized, ct);
            if (pending == null || pending.ExpiresAt < now)
                return RegistrationOutcome.Fail(RegistrationStatus.NotFound);

            var wait = RemainingCooldownSeconds(pending, now);
            if (wait > 0) return RegistrationOutcome.Wait(wait);

            return await IssueTokenAndSendAsync(pending, isNew: false, ct);
        }

        public async Task<RegistrationOutcome> ConfirmEmailAsync(string token, CancellationToken ct = default)
        {
            var hash = HashToken(token.Trim());
            var now = DateTime.UtcNow;

            var pending = await db.PendingRegistrations.FirstOrDefaultAsync(p => p.TokenHash == hash, ct);
            if (pending == null)
                return RegistrationOutcome.Fail(RegistrationStatus.InvalidOrExpiredToken);

            if (pending.ExpiresAt < now)
            {
                db.PendingRegistrations.Remove(pending);
                await db.SaveChangesAsync(ct);
                return RegistrationOutcome.Fail(RegistrationStatus.InvalidOrExpiredToken);
            }

            // Someone (e.g. GitHub login) may have created an account with this address in the meantime.
            if (await userManager.FindByEmailAsync(pending.Email) != null)
            {
                db.PendingRegistrations.Remove(pending);
                await db.SaveChangesAsync(ct);
                return RegistrationOutcome.Fail(RegistrationStatus.EmailAlreadyRegistered);
            }

            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            var user = new User
            {
                UserName = pending.Email,
                Email = pending.Email,
                EmailConfirmed = true,
                PasswordHash = pending.PasswordHash,
                FirstName = pending.FirstName,
                LastName = pending.LastName,
                ContactNumber = pending.ContactNumber,
                StudentNumber = pending.StudentNumber,
                EntryYear = pending.EntryYear,
                EducationLevel = pending.EducationLevel,
                University = UniversityName,
                TelegramId = pending.TelegramId
            };

            // No password argument on purpose: the already-hashed password is assigned above.
            var created = await userManager.CreateAsync(user);
            if (!created.Succeeded)
            {
                logger.LogError("Creating user from pending registration {PendingId} failed: {Errors}",
                    pending.Id, string.Join("; ", created.Errors.Select(e => e.Code)));
                return RegistrationOutcome.Fail(RegistrationStatus.Failed);
            }

            var roleResult = await userManager.AddToRoleAsync(user, DefaultRole);
            if (!roleResult.Succeeded)
            {
                logger.LogError("Adding role to user from pending registration {PendingId} failed: {Errors}",
                    pending.Id, string.Join("; ", roleResult.Errors.Select(e => e.Code)));
                return RegistrationOutcome.Fail(RegistrationStatus.Failed);
            }

            db.PendingRegistrations.Remove(pending);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return RegistrationOutcome.Ok(0);
        }

        // ------------------------------------------------------------------ helpers

        private async Task<RegistrationOutcome> IssueTokenAndSendAsync(PendingRegistration pending, bool isNew, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

            pending.TokenHash = HashToken(token);
            pending.ExpiresAt = now + TokenLifetime;
            pending.LastSentAt = now;

            if (isNew) db.PendingRegistrations.Add(pending);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                // Two simultaneous requests for the same address hit the unique index on Email.
                logger.LogWarning(ex, "Concurrent registration attempt for the same address.");
                return RegistrationOutcome.Wait((int)ResendCooldown.TotalSeconds);
            }

            try
            {
                var link = $"{FrontendBaseUrl()}/confirm-email?token={token}";
                await emailService.SendAsync(
                    pending.Email,
                    "تأیید ایمیل و تکمیل ثبت‌نام در انجمن علمی ACM",
                    BuildEmailHtml(pending.FirstName, link),
                    isHtml: true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Sending the confirmation email failed.");

                if (isNew)
                    db.PendingRegistrations.Remove(pending);
                else
                    pending.LastSentAt = now - ResendCooldown; // let the person retry immediately

                await db.SaveChangesAsync(CancellationToken.None);
                return RegistrationOutcome.Fail(RegistrationStatus.EmailSendFailed);
            }

            return RegistrationOutcome.Ok((int)ResendCooldown.TotalSeconds);
        }

        private async Task<bool> IsPasswordValidAsync(User probe, string password)
        {
            foreach (var validator in userManager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(userManager, probe, password);
                if (!result.Succeeded) return false;
            }
            return true;
        }

        private Task PurgeExpiredAsync(DateTime now, CancellationToken ct) =>
            db.PendingRegistrations.Where(p => p.ExpiresAt < now).ExecuteDeleteAsync(ct);

        private string FrontendBaseUrl()
        {
            var url = configuration["Frontend:BaseUrl"];
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException("Configuration value 'Frontend:BaseUrl' is missing.");
            return url.TrimEnd('/');
        }

        private static int RemainingCooldownSeconds(PendingRegistration pending, DateTime now)
        {
            var elapsed = now - pending.LastSentAt;
            return elapsed >= ResendCooldown ? 0 : (int)Math.Ceiling((ResendCooldown - elapsed).TotalSeconds);
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        private static string? NormalizeTelegramId(string? id)
        {
            var value = id?.Trim().TrimStart('@');
            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static string HashToken(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        private static string BuildEmailHtml(string firstName, string link)
        {
            var name = WebUtility.HtmlEncode(firstName);
            var href = WebUtility.HtmlEncode(link);
            var hours = (int)TokenLifetime.TotalHours;

            return $@"<div dir=""rtl"" style=""font-family:Tahoma,Arial,sans-serif;line-height:2;color:#111;text-align:right"">
  <p>{name} عزیز، سلام</p>
  <p>برای تکمیل ثبت‌نام در سایت انجمن علمی ACM دانشگاه صنعتی اصفهان، روی دکمه‌ی زیر کلیک کنید:</p>
  <p><a href=""{href}"" style=""display:inline-block;padding:10px 24px;background:#0e7490;color:#ffffff;border-radius:8px;text-decoration:none"">تأیید ایمیل</a></p>
  <p>اگر دکمه کار نکرد، این آدرس را در مرورگر خود باز کنید:<br><span dir=""ltr"">{href}</span></p>
  <p>این لینک تا {hours} ساعت معتبر است و حساب شما تنها پس از تأیید ایمیل ساخته می‌شود. اگر شما ثبت‌نام نکرده‌اید، این ایمیل را نادیده بگیرید.</p>
</div>";
        }
    }
}
