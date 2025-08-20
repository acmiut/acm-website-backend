using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Mvc;
using ZarinPal.Class;

namespace IUT_ACM_Solution.Controllers
{
    public class TeamController : Controller
    {
        private Payment _payment;
        private Authority _authority;
        private Transactions _transactions;

        public TeamController()
        {
            var expose = new Expose();
            _payment = expose.CreatePayment();
            _authority = expose.CreateAuthority();
            _transactions = expose.CreateTransactions();
        }
        [HttpPost("register-event")]
        public IActionResult RegisterTeam(Guid eventId, TeamDto teamDto)
        {
            if (!ModelState.IsValid) return View(teamDto);

            var team = new Team
            {
                TeamName = teamDto.TeamName,
                TeamMemberEmails = teamDto.TeamMemberEmails,
                EventId = eventId,
                HasPaid = false
            };

            return RedirectToAction("Payment", team);
        }


    }
}
