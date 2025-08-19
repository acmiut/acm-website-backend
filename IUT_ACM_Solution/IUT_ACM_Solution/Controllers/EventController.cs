using IUT_ACM_Solution.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace IUT_ACM_Solution.Controllers
{

    [Route("events")]
    public class EventController : Controller
    {
        private readonly IEventManagement eventManagement;

        public EventController(IEventManagement eventManagement)
        {
            this.eventManagement = eventManagement;
        }
        public async Task<IActionResult> Index()
        {
            var events = await eventManagement.GetEventsAsync();
            return View(events);
        }

        [HttpPost("add-event")]
        public async Task<IActionResult> AddEvent()
        {

        }
    }
}
