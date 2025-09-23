using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;
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

        [HttpGet("add-event")]
        public async Task<IActionResult> AddEvent()
        {
            return View();
        }

        [HttpPost("add-event")]
        public async Task<IActionResult> AddEvent([FromForm] EventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                return View(eventDto);
            }

            string imageUrl = string.Empty;

            if (eventDto.ImageFile != null && eventDto.ImageFile.Length > 0)
            {
                var extension = Path.GetExtension(eventDto.ImageFile.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Unsupported image format.");
                    return View(eventDto);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "events");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);

                try
                {
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await eventDto.ImageFile.CopyToAsync(stream);
                    imageUrl = $"/images/events/{fileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ImageFile", $"Image upload failed: {ex.Message}");
                    return View(eventDto);
                }
            }

            var @event = new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                ImageUrl = imageUrl,
                Location = eventDto.Location,
                StartDate = eventDto.StartDate,
                EndDate = eventDto.EndDate,
                EventType = eventDto.EventType,
                Status = eventDto.Status,
                Capacity = eventDto.Capacity,
                IsTeamBased = eventDto.IsTeamBased,
                MaxTeamMembers = eventDto.MaxTeamMembers,
                RequiresRegistration = eventDto.RequiresRegistration,
                Fee = eventDto.Fee,
                OrganizedBy = eventDto.OrganizedBy,
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            try
            {
                await eventManagement.CreateEventAsync(@event);
            }
            catch(Exception ex)
            {
                return View(eventDto);
            }

            return RedirectToAction("Index", "Event");
        }

        [HttpPut("update-event")]
        public async Task<IActionResult> UpdateEvent(Guid eventId, [FromForm] EventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                return View(eventDto);
            }

            var @event = await eventManagement.GetEventByIdAsync(eventId);
            if (@event == null)
            {
                ModelState.AddModelError("", "Event not found.");
                return View(eventDto);
            }

            @event.Title = eventDto.Title;
            @event.Description = eventDto.Description;

            if (eventDto.ImageFile != null && eventDto.ImageFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{eventDto.ImageFile.FileName}";
                var filePath = Path.Combine("wwwroot/images/events", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await eventDto.ImageFile.CopyToAsync(stream);
                }

                @event.ImageUrl = $"/images/events/{fileName}";
            }

            @event.Location = eventDto.Location;
            @event.StartDate = eventDto.StartDate;
            @event.EndDate = eventDto.EndDate;
            @event.EventType = eventDto.EventType;
            @event.Status = eventDto.Status;
            @event.Capacity = eventDto.Capacity;
            @event.IsTeamBased = eventDto.IsTeamBased;
            @event.MaxTeamMembers = eventDto.MaxTeamMembers;
            @event.RequiresRegistration = eventDto.RequiresRegistration;
            @event.Fee = eventDto.Fee; 
            @event.OrganizedBy = eventDto.OrganizedBy;

            await eventManagement.UpdateEventAsync(eventId, @event);

            return RedirectToAction("Details", new { id = eventId });
        }
        
        [HttpGet("get-events")]    
        
        public async Task<IActionResult> GetAllEvents()
        {
            try
            {
                var events = await eventManagement.GetEventsAsync();
                return View(events);
            }
            catch (Exception ex)
            { 
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("get-event/{eventId}")]
        public async Task<IActionResult> GetEventById([FromRoute] Guid eventId)
        {
            try
            {
                var @event = await eventManagement.GetEventByIdAsync(eventId);
                if (@event == null) return NotFound();
                return View(@event);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete-event/{eventId}")]
        public async Task<IActionResult> DeleteEvent([FromRoute] Guid eventId)
        {
            try
            {
                await eventManagement.DeleteEventAsync(eventId);
                return RedirectToAction("");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
