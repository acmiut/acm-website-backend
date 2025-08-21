using Dto.Payment;
using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ZarinPal.Class;

namespace IUT_ACM_Solution.Controllers
{
    public class TeamController : Controller
    {
        private readonly IEventManagement eventManagement;
        private readonly ITeamManagement teamManagement;
        private readonly UserManager<User> userManager;
        private readonly IOptions<ZarinPalSetting> zarinPalOptions;
        private Payment _payment;
        private Authority _authority;
        private Transactions _transactions;

        public TeamController(
            IEventManagement eventManagement, 
            ITeamManagement teamManagement, 
            UserManager<User> userManager,
            IOptions<ZarinPalSetting> zarinPalOptions)
        {
            var expose = new Expose();
            _payment = expose.CreatePayment();
            _authority = expose.CreateAuthority();
            _transactions = expose.CreateTransactions();
            this.eventManagement = eventManagement;
            this.teamManagement = teamManagement;
            this.userManager = userManager;
            this.zarinPalOptions = zarinPalOptions;
        }
        [HttpPost("register-event")]
        public async Task<IActionResult> RegisterTeam(Guid eventId, TeamDto teamDto)
        {
            if (!ModelState.IsValid) return View(teamDto);

            var @event = await eventManagement.GetEventByIdAsync(eventId);
            if (@event == null) return NotFound();

            var team = new Team
            {
                TeamName = teamDto.TeamName,
                TeamMemberEmails = teamDto.TeamMemberEmails,
                EventId = eventId,
                HasPaid = false,
                FeeToPay = @event.Fee
            };

            await teamManagement.CreateTeamAsync(team);

            var user = await userManager.FindByEmailAsync(User.Identity?.Name ?? "");
            var callbackUrl = Url.Action("CompleteTeamRegisteration", "Team", new { teamId = team.Id, eventId = eventId }, Request.Scheme);

            var req = await _payment.Request(new DtoRequest()
            {
                Amount = team.FeeToPay,
                CallbackUrl = callbackUrl,
                Description = @event.Title,
                Email = User.Identity?.Name,
                Mobile = user?.ContactNumber,
                MerchantId = zarinPalOptions.Value.MerchantId
            }, ZarinPal.Class.Payment.Mode.sandbox);

            team.Authority = req.Authority;
            await teamManagement.UpdateTeamAsync(team.Id, team);

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
                MerchantId = zarinPalOptions.Value.MerchantId
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
