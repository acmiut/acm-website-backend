using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.Models
{
    /// <summary>
    /// A sign-up that has been submitted but whose email address is not confirmed yet.
    /// The person is NOT a <see cref="User"/> until the confirmation link is used:
    /// at that moment this row is turned into a real user and deleted.
    /// </summary>
    public class PendingRegistration
    {
        public Guid Id { get; set; }

        /// <summary>Trimmed + lower-cased. Unique: one pending sign-up per address.</summary>
        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        /// <summary>SHA-256 (hex) of the token sent by email. The raw token is never stored.</summary>
        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>Output of IPasswordHasher. The plain password is never stored.</summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(12)]
        public string StudentNumber { get; set; } = string.Empty;

        /// <summary>Solar-hijri (Persian) year of entry, e.g. 1403.</summary>
        public int EntryYear { get; set; }

        public EducationLevel EducationLevel { get; set; } = EducationLevel.Undergraduate;

        [MaxLength(255)]
        public string? TelegramId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        /// <summary>Last time a confirmation email was sent (used for the resend cool-down).</summary>
        public DateTime LastSentAt { get; set; }
    }
}
