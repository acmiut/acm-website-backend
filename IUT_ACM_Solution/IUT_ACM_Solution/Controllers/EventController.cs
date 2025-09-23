using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IUT_ACM_Solution.Controllers
{

    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventManagement _eventManagement;

        public EventController(IEventManagement eventManagement)
        {
            _eventManagement = eventManagement;
        }

        // GET: api/events
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            try
            {
                var events = await _eventManagement.GetEventsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                // Log the exception details
                // logger.LogError(ex, "Failed to retrieve events.");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/events/{eventId}
        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetEventById([FromRoute] Guid eventId)
        {
            var @event = await _eventManagement.GetEventByIdAsync(eventId);
            if (@event == null)
            {
                return NotFound();
            }

            return Ok(@event);
        }

        // POST: api/events
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromForm] EventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var imageUrl = await HandleImageUpload(eventDto.ImageFile, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                await _eventManagement.CreateEventAsync(@event);
                return CreatedAtAction(nameof(GetEventById), new { eventId = @event.Id }, @event);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/events/{eventId}
        [HttpPut("{eventId}")]
        public async Task<IActionResult> UpdateEvent([FromRoute] Guid eventId, [FromForm] EventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var @event = await _eventManagement.GetEventByIdAsync(eventId);
            if (@event == null)
            {
                return NotFound();
            }

            @event.Title = eventDto.Title;
            @event.Description = eventDto.Description;

            if (eventDto.ImageFile != null)
            {
                @event.ImageUrl = await HandleImageUpload(eventDto.ImageFile, ModelState);
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
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
            @event.LastUpdated = DateTime.UtcNow;

            try
            {
                await _eventManagement.UpdateEventAsync(eventId, @event);
                return NoContent(); // 204 No Content is standard for successful updates
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/events/{eventId}
        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteEvent([FromRoute] Guid eventId)
        {
            var @event = await _eventManagement.GetEventByIdAsync(eventId);
            if (@event == null)
            {
                return NotFound();
            }

            try
            {
                await _eventManagement.DeleteEventAsync(eventId);
                return NoContent(); // 204 No Content is standard for successful deletions
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<string> HandleImageUpload(IFormFile? imageFile, ModelStateDictionary modelState)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return string.Empty;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                modelState.AddModelError("ImageFile", "Unsupported image format. Allowed formats: JPG, JPEG, PNG, WEBP.");
                return string.Empty;
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "events");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                return $"/images/events/{fileName}";
            }
            catch (Exception ex)
            {
                modelState.AddModelError("ImageFile", $"Image upload failed: {ex.Message}");
                return string.Empty;
            }
        }
    }
}