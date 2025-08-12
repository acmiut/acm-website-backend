using Microsoft.AspNetCore.Mvc;

namespace IUT_ACM_Solution.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
