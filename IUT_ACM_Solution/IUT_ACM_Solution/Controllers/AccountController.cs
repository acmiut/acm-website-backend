using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IUT_ACM_Solution.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;

        public AccountController(UserManager<User> userManager)
        {
            this.userManager = userManager;
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

    }
}
