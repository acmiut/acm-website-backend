using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Models;
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

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet("register")]
        public IActionResult Register() => View();

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)  return View(registerDto);

            var user = new User
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                StudentNumber = registerDto.StudentNumber,
                EntryYear = registerDto.EntryYear,
                EducationLevel = registerDto.EducationLevel,
                University = registerDto.University,
                TelegramId = registerDto.TelegramId
            };

            var res = await userManager.CreateAsync(user, registerDto.Password);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");
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

        [HttpGet("login")]
        public IActionResult Login() => View();

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
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
    }
}
