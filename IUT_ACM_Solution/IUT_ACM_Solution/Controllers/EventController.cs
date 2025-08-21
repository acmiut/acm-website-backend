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
                BasePrice = eventDto.BasePrice,
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
    
    }
}
