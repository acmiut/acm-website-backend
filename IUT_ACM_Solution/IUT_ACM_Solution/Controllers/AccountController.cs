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
                return RedirectToAction("Login");
            }

            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return Redirect(returnUrl ?? "/");
            }

            // If the user does not have an account, create one
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = new User { UserName = email, Email = email, EmailConfirmed = true };

            var createResult = await userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                await userManager.AddLoginAsync(user, info);
                await signInManager.SignInAsync(user, isPersistent: false);
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
