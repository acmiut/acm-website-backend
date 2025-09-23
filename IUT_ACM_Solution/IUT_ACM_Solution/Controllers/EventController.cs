using IUT_ACM_Solution.DTOs;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IUT_ACM_Solution.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventManagement _eventManagement;

        public EventController(IEventManagement eventManagement)
        {
            _eventManagement = eventManagement;
        }

        // GET: /Event
        public async Task<IActionResult> Index()
        {
            var events = await _eventManagement.GetEventsAsync();
            return View(events);
        }

        // GET: /Event/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var @event = await _eventManagement.GetEventByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        // GET: /Event/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                return View(eventDto);
            }

            var imageUrl = await HandleImageUpload(eventDto.ImageFile, ModelState);
            if (!ModelState.IsValid)
            {
                return View(eventDto);
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

            await _eventManagement.CreateEventAsync(@event);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Event/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var @event = await _eventManagement.GetEventByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            var dto = new EventDto
            {
                Title = @event.Title,
                Description = @event.Description,
                Location = @event.Location,
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                EventType = @event.EventType,
                Status = @event.Status,
                Capacity = @event.Capacity,
                IsTeamBased = @event.IsTeamBased,
                MaxTeamMembers = @event.MaxTeamMembers,
                RequiresRegistration = @event.RequiresRegistration,
                Fee = @event.Fee,
                OrganizedBy = @event.OrganizedBy
            };

            return View(dto);
        }

        // POST: /Event/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                return View(eventDto);
            }

            var @event = await _eventManagement.GetEventByIdAsync(id);
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
                    return View(eventDto);
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

            await _eventManagement.UpdateEventAsync(id, @event);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Event/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var @event = await _eventManagement.GetEventByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        // POST: /Event/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _eventManagement.DeleteEventAsync(id);
            return RedirectToAction(nameof(Index));
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
