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
        private readonly UserManager<User> userManager;
        private Payment _payment;
        private Authority _authority;
        private Transactions _transactions;

        public TeamController(IEventManagement eventManagement, UserManager<User> userManager)
        {
            var expose = new Expose();
            _payment = expose.CreatePayment();
            _authority = expose.CreateAuthority();
            _transactions = expose.CreateTransactions();
            this.eventManagement = eventManagement;
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

            var @event =await eventManagement.GetEventByIdAsync(eventId);
            int price = @event.BasePrice; // should add logic to apply discounts here or somewhere else

            var user = await userManager.FindByEmailAsync(User.Identity?.Name ?? "");

            var req = await _payment.Request(new DtoRequest()
            {
                Amount = @event.BasePrice,
                CallbackUrl = "",
                Description = @event.Title,
                Email = User.Identity?.Name,
                Mobile = user?.ContactNumber,
                MerchantId = ""
            }, ZarinPal.Class.Payment.Mode.sandbox);

            return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{req.Authority}");
        }


    }
}
