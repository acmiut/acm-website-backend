using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IUT_ACM_Solution.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IEmailService emailService;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailService emailService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home"); 
            }

            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)  return View(registerDto);

            var user = new User
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                ContactNumber = registerDto.ContactNumber
            };

            var res = await userManager.CreateAsync(user, registerDto.Password);
            if (res.Succeeded)
            {
                var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, token = confirmationToken },
                    protocol: Request.Scheme
                );

                await emailService.SendAsync(
                    user.Email,
                    "Please confirm your email",
                    $"Please click on this link to confirm your email address: {confirmationLink}");

                return RedirectToAction("Login");
            }
            else
            {
                foreach (var error  in res.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }
                return View(registerDto);
            }
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            bool isEmailConfirmed = false;
            var user = await this.userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await this.userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    isEmailConfirmed = true;
                }
            }
            return View(isEmailConfirmed);
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid) return View(loginDto);

            var res = await signInManager.PasswordSignInAsync(
                loginDto.Email,
                loginDto.Password,
                loginDto.RememberMe,
                false);

            if (res.Succeeded)
            {
                return Redirect("/");
            }
            else
            {
                if (res.IsLockedOut)
                {
                    ModelState.AddModelError("Login", "You are locked out.");
                }
                else
                {
                    ModelState.AddModelError("Login", "Failed to login.");
                }
                return View(loginDto);
            }
        }

        [HttpGet("forget-password")]
        public IActionResult ForgetPassword() => View();

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(string Email)
        {
            var user = await userManager.FindByNameAsync(Email);

            if (user != null)
            {
                var resetPasswordToken = await userManager.GeneratePasswordResetTokenAsync(user);

                var resetLink = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { userId = user.Id, token = resetPasswordToken },
                    protocol: Request.Scheme
                );

                await emailService.SendAsync(
                    user.Email??"",
                    "Reset Password",
                    $"Please click on this link to reset your password: {resetLink}");
            }

            return RedirectToAction("Login");

        }

        [HttpGet("reset-password")]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("ForgetPassword");
            }

            var model = new ResetPasswordDto { UserId = userId, Token = token };
            return View(model);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (ModelState.IsValid)
            {
                if (resetPasswordDto.NewPassword == resetPasswordDto.ConfirmPassword)
                {
                    bool isTokenValid = false;

                    var user = await userManager.FindByIdAsync(resetPasswordDto.UserId);
                    if (user != null)
                    {
                        var result = await userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
                        if (result.Succeeded)
                        {
                            isTokenValid = true;
                        }
                    }
                    return View("ResetResult", isTokenValid);

                }
                else
                {
                    ModelState.AddModelError("", "Please confirm your password correctly.");
                }
            }

            return View(resetPasswordDto);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Redirect("/Account/Login");
        }

        [HttpPost("external-login")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction("Login");
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return RedirectToAction("Login");
            }

            // Try to sign in with external login
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return Redirect(returnUrl ?? "/");
            }

            // Get email from external provider
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                ModelState.AddModelError(string.Empty, "Email not received from external provider.");
                return RedirectToAction("Login");
            }

            // Check if a user with this email already exists
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // Link external login to existing user
                var linkResult = await userManager.AddLoginAsync(existingUser, info);
                if (linkResult.Succeeded)
                {
                    await signInManager.SignInAsync(existingUser, isPersistent: false);
                    return Redirect(returnUrl ?? "/");
                }

                foreach (var error in linkResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return RedirectToAction("Login");
            }

            // No user exists — create a new one
            var newUser = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "External", // You can customize this
                LastName = "User",
                ContactNumber = "",
                EntryYear = DateTime.UtcNow.Year,
                EducationLevel = EducationLevel.Undergraduate,
                University = "Unknown"
            };

            var createResult = await userManager.CreateAsync(newUser);
            if (createResult.Succeeded)
            {
                await userManager.AddLoginAsync(newUser, info);
                await userManager.AddToRoleAsync(newUser, "User");
                await signInManager.SignInAsync(newUser, isPersistent: false);
                return Redirect(returnUrl ?? "/");
            }

            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Login");
        }

        [HttpGet("user-profile")]
        public async Task<IActionResult> UserProfile()
        {
            var user = await userManager.FindByNameAsync(User.Identity?.Name??"");
            if (user != null)
            {
                UserProfileDto userProfile = new UserProfileDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    StudentNumber = user.StudentNumber,
                    ContactNumber = user.ContactNumber,
                    EntryYear = user.EntryYear,
                    EducationLevel = user.EducationLevel,
                    University = user.University,
                    TelegramId = user.TelegramId
                };

                return View(userProfile);
            }
            return NotFound();
        }

        [HttpPost("user-profile")]
        public async Task<IActionResult> UserProfile(UserProfileDto userProfile)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(User.Identity?.Name ?? "");
                if (user != null)
                {
                    user.FirstName = userProfile.FirstName;
                    user.LastName = userProfile.LastName;
                    user.ContactNumber = userProfile.ContactNumber;
                    user.StudentNumber = userProfile.StudentNumber;
                    user.EntryYear = userProfile.EntryYear;
                    user.University = userProfile.University;
                    user.TelegramId = userProfile.TelegramId;

                    var result = await userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Error occured while updating the profile.");
                    }
                }
            }
            return View(userProfile);
        }
    
    }
}
