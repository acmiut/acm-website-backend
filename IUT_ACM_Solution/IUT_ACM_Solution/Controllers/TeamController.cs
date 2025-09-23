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
    [Route("api/teams")]
    [ApiController]
    public class TeamController : ControllerBase
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

        // GET: api/teams
        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            var teams = await _teamManagement.GetTeamsAsync();
            return Ok(teams);
        }

        // POST: api/teams/register/{eventId}
        [HttpPost("register/{eventId}")]
        public async Task<IActionResult> RegisterTeam(Guid eventId, [FromBody] TeamDto teamDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
            var callbackUrl = Url.Action("CompleteTeamRegisteration", "Team", new { teamId = team.Id, eventId = eventId }, Request.Scheme);
            if (callbackUrl == null)
            {
                // This could happen if the route isn't configured correctly.
                return StatusCode(500, "Could not generate callback URL.");
            }

            var req = await _payment.Request(new DtoRequest()
            {
                Amount = team.FeeToPay,
                CallbackUrl = callbackUrl,
                Description = @event.Title,
                Email = User.Identity?.Name,
                Mobile = user?.ContactNumber,
                MerchantId = _zarinPalOptions.Value.MerchantId
            }, ZarinPal.Class.Payment.Mode.sandbox);

            team.Authority = req.Authority;
            await _teamManagement.UpdateTeamAsync(team.Id, team);

            return Ok(new { paymentUrl = $"https://sandbox.zarinpal.com/pg/StartPay/{req.Authority}" });
        }

        // GET: api/teams/complete-registration
        [HttpGet("complete-registration")]
        public async Task<IActionResult> CompleteTeamRegistration(Guid teamId, string authority, string status)
        {
            var team = await _teamManagement.GetTeamByIdAsync(teamId);
            if (team == null || team.Authority != authority)
            {
                return BadRequest(new { message = "Invalid payment reference." });
            }

            if (status != "OK")
            {
                return BadRequest(new { message = "Payment was not successful." });
            }

            var verification = await _payment.Verification(new DtoVerification()
            {
                Amount = team.FeeToPay,
                Authority = authority,
                MerchantId = _zarinPalOptions.Value.MerchantId
            }, ZarinPal.Class.Payment.Mode.sandbox);

            if (verification.Status == 100)
            {
                team.HasPaid = true;
                await _teamManagement.UpdateTeamAsync(team.Id, team);
                return Ok(new { trackingCode = verification.RefId, message = "Payment successful." });
            }
            else
            {
                return BadRequest(new { message = "Payment verification failed." });
            }
        }

        // GET: api/teams/{teamId}
        [HttpGet("{teamId}")]
        public async Task<IActionResult> GetTeamById(Guid teamId)
        {
            var team = await _teamManagement.GetTeamByIdAsync(teamId);
            if (team == null)
            {
                return NotFound();
            }
            return Ok(team);
        }

        // PUT: api/teams/{teamId}
        [HttpPut("{teamId}")]
        public async Task<IActionResult> UpdateTeam(Guid teamId, [FromBody] TeamDto teamDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var team = await _teamManagement.GetTeamByIdAsync(teamId);
            if (team == null)
            {
                return NotFound();
            }

            // Check if the update is valid (e.g., uniqueness of team name, rules about members)
            // This logic is missing in the original and should be implemented in teamManagement.

            team.TeamName = teamDto.TeamName;
            team.TeamMemberEmails = teamDto.TeamMemberEmails;

            try
            {
                await _teamManagement.UpdateTeamAsync(teamId, team);
                return NoContent(); // 204 No Content is standard for successful updates
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/teams/{teamId}
        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(Guid teamId)
        {
            var team = await _teamManagement.GetTeamByIdAsync(teamId);
            if (team == null)
            {
                return NotFound();
            }

            try
            {
                await _teamManagement.DeleteTeamAsync(teamId);
                return NoContent(); // 204 No Content is standard for successful deletions
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }
    }
}