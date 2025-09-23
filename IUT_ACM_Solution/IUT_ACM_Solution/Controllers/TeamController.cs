using Dto.Payment;
using Dto.Response.Payment;
using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;
using IUT_ACM_Solution.Services;
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
        public async Task<IActionResult> CompleteTeamRegisteration(Guid teamId, string authority, string status)
        {
            var team = await teamManagement.GetTeamByIdAsync(teamId);
            if (team == null || team.Authority != authority)
            {
                return RedirectToAction("PaymentFailed", new { message = "Invalid payment reference." });
            }

            if (status != "OK")
            {
                return RedirectToAction("PaymentFailed", new { message = "Payment was not successful." });
            }

            var verification = await _payment.Verification(new DtoVerification()
            {
                Amount = team.FeeToPay,
                Authority = authority,
                MerchantId = zarinPalOptions.Value.MerchantId
            }, ZarinPal.Class.Payment.Mode.sandbox);

            if (verification.Status == 100) 
            {
                team.HasPaid = true;
                await teamManagement.UpdateTeamAsync(team.Id, team);
                return RedirectToAction("PaymentSuccess", new { trackingCode = verification.RefId });
            }
            else
            {
                return RedirectToAction("PaymentFailed", new { message = "Payment verification failed." });
            }
        }

        [HttpPut("update-team")]
        public async Task<IActionResult> UpdateTeam(Guid eventId, Guid teamId, TeamDto teamDto)
        {
            try
            {
                if (!ModelState.IsValid) return View(teamDto);

                var @event = await eventManagement.GetEventByIdAsync(eventId);
                if (@event == null) return View(teamDto);

                var team = await teamManagement.GetTeamByIdAsync(teamId);
                if (team == null) return View(teamDto);

                if (team.EventId != eventId) return BadRequest();


                // check the update is valid (in case of uniqness of the team name and rules about the members)
                team.TeamName = teamDto.TeamName;
                team.TeamMemberEmails = teamDto.TeamMemberEmails;

                await teamManagement.UpdateTeamAsync(teamId, team);
                return RedirectToAction("");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete-team")]
        public async Task<IActionResult> DeleteTeam(Guid teamId)
        {
            try
            {
                var team = await teamManagement.GetTeamByIdAsync(teamId);
                if (team == null) return BadRequest();

                await teamManagement.DeleteTeamAsync(teamId);
                return RedirectToAction("/");
            }
            catch (Exception ex) 
            {
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult PaymentSuccess(string trackingCode)
        {
            return View("PaymentSuccess", trackingCode);
        }
        public IActionResult PaymentFailed(string message)
        {
            return View("PaymentFailed", message);
        }
    }
}
