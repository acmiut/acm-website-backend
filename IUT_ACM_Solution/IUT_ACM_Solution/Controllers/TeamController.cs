using Dto.Payment;
using Dto.Response.Payment;
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
        private readonly IEventManagement _eventManagement;
        private readonly ITeamManagement _teamManagement;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<ZarinPalSetting> _zarinPalOptions;
        private readonly Payment _payment;
        private readonly Authority _authority;
        private readonly Transactions _transactions;

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
            _eventManagement = eventManagement;
            _teamManagement = teamManagement;
            _userManager = userManager;
            _zarinPalOptions = zarinPalOptions;
        }

        // GET: /Team
        public async Task<IActionResult> Index()
        {
            var teams = await _teamManagement.GetTeamsAsync();
            return View(teams);
        }

        // GET: /Team/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var team = await _teamManagement.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // GET: /Team/Register/{eventId}
        public async Task<IActionResult> Register(Guid eventId)
        {
            var @event = await _eventManagement.GetEventByIdAsync(eventId);
            if (@event == null)
            {
                return NotFound();
            }

            ViewBag.Event = @event;
            return View(new TeamDto());
        }

        // POST: /Team/Register/{eventId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Guid eventId, TeamDto teamDto)
        {
            if (!ModelState.IsValid)
            {
                return View(teamDto);
            }

            var @event = await _eventManagement.GetEventByIdAsync(eventId);
            if (@event == null)
            {
                return NotFound("Event not found.");
            }

            var team = new Team
            {
                TeamName = teamDto.TeamName,
                TeamMemberEmails = teamDto.TeamMemberEmails,
                EventId = eventId,
                HasPaid = false,
                FeeToPay = @event.Fee
            };

            await _teamManagement.CreateTeamAsync(team);

            var user = await _userManager.FindByEmailAsync(User.Identity?.Name ?? "");
            var callbackUrl = Url.Action("CompleteRegistration", "Team", new { teamId = team.Id, eventId = eventId }, Request.Scheme);

            var req = await _payment.Request(new DtoRequest()
            {
                Amount = team.FeeToPay,
                CallbackUrl = callbackUrl!,
                Description = @event.Title,
                Email = User.Identity?.Name,
                Mobile = user?.ContactNumber,
                MerchantId = _zarinPalOptions.Value.MerchantId
            }, Payment.Mode.sandbox);

            team.Authority = req.Authority;
            await _teamManagement.UpdateTeamAsync(team.Id, team);

            // Instead of returning JSON, redirect user to payment page
            return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{req.Authority}");
        }

        // GET: /Team/CompleteRegistration
        public async Task<IActionResult> CompleteRegistration(Guid teamId, string authority, string status)
        {
            var team = await _teamManagement.GetTeamByIdAsync(teamId);
            if (team == null || team.Authority != authority)
            {
                ViewBag.Message = "Invalid payment reference.";
                return View("PaymentFailed");
            }

            if (status != "OK")
            {
                ViewBag.Message = "Payment was not successful.";
                return View("PaymentFailed");
            }

            var verification = await _payment.Verification(new DtoVerification()
            {
                Amount = team.FeeToPay,
                Authority = authority,
                MerchantId = _zarinPalOptions.Value.MerchantId
            }, Payment.Mode.sandbox);

            if (verification.Status == 100)
            {
                team.HasPaid = true;
                await _teamManagement.UpdateTeamAsync(team.Id, team);
                ViewBag.TrackingCode = verification.RefId;
                return View("PaymentSuccess", team);
            }
            else
            {
                ViewBag.Message = "Payment verification failed.";
                return View("PaymentFailed");
            }
        }

        // GET: /Team/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var team = await _teamManagement.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            var dto = new TeamDto
            {
                TeamName = team.TeamName,
                TeamMemberEmails = team.TeamMemberEmails
            };

            return View(dto);
        }

        // POST: /Team/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TeamDto teamDto)
        {
            if (!ModelState.IsValid)
            {
                return View(teamDto);
            }

            var team = await _teamManagement.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            team.TeamName = teamDto.TeamName;
            team.TeamMemberEmails = teamDto.TeamMemberEmails;

            await _teamManagement.UpdateTeamAsync(id, team);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Team/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var team = await _teamManagement.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // POST: /Team/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _teamManagement.DeleteTeamAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
