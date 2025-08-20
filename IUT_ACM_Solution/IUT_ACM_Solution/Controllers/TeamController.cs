using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Mvc;

namespace IUT_ACM_Solution.Controllers
{
    public class TeamController : Controller
    {
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
