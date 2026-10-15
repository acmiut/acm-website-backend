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

            var viewModel = new TeamRegisterationViewModel
            {
                EventId = @event.Id,
                EventTitle = @event.Title,
                Fee = @event.Fee
            };

            return View(viewModel);
        }

        // POST: /Team/Register/{eventId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(TeamRegisterationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var @event = await _eventManagement.GetEventByIdAsync(model.EventId);
            if (@event == null)
                return NotFound("Event not found.");

            var team = new Team
            {
                TeamName = model.Team.TeamName,
                TeamMemberEmails = model.Team.TeamMemberEmails,
                EventId = model.EventId,
                HasPaid = false,
                FeeToPay = @event.Fee
            };

            await _teamManagement.CreateTeamAsync(team);

            var user = await _userManager.FindByEmailAsync(User.Identity?.Name ?? "");
            var callbackUrl = Url.Action("CompleteRegistration", "Team", new { teamId = team.Id, eventId = model.EventId }, Request.Scheme);

            using var httpClient = new HttpClient();
            var requestBody = new
            {
                merchant_id = _zarinPalOptions.Value.MerchantId,
                amount = team.FeeToPay,
                description = @event.Title,
                callback_url = callbackUrl,
                metadata = new
                {
                    email = User.Identity?.Name,
                    mobile = user?.ContactNumber
                }
            };

            var response = await httpClient.PostAsJsonAsync("https://sandbox.zarinpal.com/pg/v4/payment/request.json", requestBody);
            var result = await response.Content.ReadFromJsonAsync<ZarinpalPaymentResponse>();

            if (result?.data?.code == 100)
            {
                team.Authority = result.data.authority;
                await _teamManagement.UpdateTeamAsync(team.Id, team);
                return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{result.data.authority}");
            }
            else
            {
                ViewBag.Message = $"Payment request failed. Code: {result?.errors?.GetRawText() ?? "Unknown"}";
                return View("PaymentFailed");
            }
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

            using var httpClient = new HttpClient();
            var verifyBody = new
            {
                merchant_id = _zarinPalOptions.Value.MerchantId,
                amount = team.FeeToPay,
                authority = authority
            };

            var response = await httpClient.PostAsJsonAsync("https://sandbox.zarinpal.com/pg/v4/payment/verify.json", verifyBody);
            var result = await response.Content.ReadFromJsonAsync<ZarinpalVerificationResponse>();

            if (result?.data?.code == 100)
            {
                team.HasPaid = true;
                await _teamManagement.UpdateTeamAsync(team.Id, team);
                ViewBag.TrackingCode = result.data.ref_id;
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
