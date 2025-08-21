using Dto.Payment;
using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZarinPal.Class;

namespace IUT_ACM_Solution.Controllers
{
    public class TeamController : Controller
    {
        private readonly IEventManagement eventManagement;
        private readonly ITeamManagement teamManagement;
        private readonly UserManager<User> userManager;
        private Payment _payment;
        private Authority _authority;
        private Transactions _transactions;

        public TeamController(IEventManagement eventManagement, ITeamManagement teamManagement, UserManager<User> userManager)
        {
            var expose = new Expose();
            _payment = expose.CreatePayment();
            _authority = expose.CreateAuthority();
            _transactions = expose.CreateTransactions();
            this.eventManagement = eventManagement;
            this.teamManagement = teamManagement;
            this.userManager = userManager;
        }
        [HttpPost("register-event")]
        public async Task<IActionResult> RegisterTeam(Guid eventId, TeamDto teamDto)
        {
            if (!ModelState.IsValid) return View(teamDto);

            var team = new Team
            {
                TeamName = teamDto.TeamName,
                TeamMemberEmails = teamDto.TeamMemberEmails,
                EventId = eventId,
                HasPaid = false
            };

            // should add better logic to apply discounts here or somewhere else
            var @event = await eventManagement.GetEventByIdAsync(eventId);
            team.FeeToPay = @event.Fee; 

            await teamManagement.CreateTeamAsync(team);
            TempData["TeamId"] = team.Id;

            var user = await userManager.FindByEmailAsync(User.Identity?.Name ?? "");

            var req = await _payment.Request(new DtoRequest()
            {
                Amount = team.FeeToPay,
                CallbackUrl = "",
                Description = @event.Title,
                Email = User.Identity?.Name,
                Mobile = user?.ContactNumber,
                MerchantId = ""
            }, ZarinPal.Class.Payment.Mode.sandbox);

            return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{req.Authority}");
        }

        [HttpGet("complete-team-registeration")]
        public async Task<IActionResult> CompleteTeamRegisteration(string authority, string status)
        {
            var team = await teamManagement.GetTeamByIdAsync(Guid.Parse(TempData["TeamId"].ToString()));
            int amount = team.FeeToPay;

            var verfication = await _payment.Verification(new DtoVerification()
            {
                Amount = amount,
                Authority = authority,
                MerchantId = ""
            }, ZarinPal.Class.Payment.Mode.sandbox);

            if (status == "OK")
            {
                team.HasPaid = true;
                await teamManagement.UpdateTeamAsync(team.Id, team);
                return View("Successful!");
            }
            else
            {
                return View(verfication.Status.ToString());
            }
        }

    }
}
