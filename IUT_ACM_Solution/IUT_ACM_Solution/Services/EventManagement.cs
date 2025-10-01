using IUT_ACM_Solution.Interfaces.Repositories;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Interfaces.Validations;
using IUT_ACM_Solution.Models;
using IUT_ACM_Solution.Repositories;
using IUT_ACM_Solution.Services.Validations;

namespace IUT_ACM_Solution.Services
{
    public class EventManagement : IEventManagement
    {
        private readonly IEventRepository eventRepository;
        private readonly IEventValidation eventValidation;

        public EventManagement(IEventRepository eventRepository, IEventValidation eventValidation)
        {
            this.eventRepository = eventRepository;
            this.eventValidation = eventValidation;
        }
        public async Task CreateEventAsync(Event @event)
        {
            var validationResult = await eventValidation.ValidateEventCreationAsync(@event);
            
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Event validation failed: {validationResult.ErrorMessage}");
            }

            @event.CreatedAt = DateTime.UtcNow;
            @event.LastUpdated = DateTime.UtcNow;

            await eventRepository.CreateEventAsync(@event);
        }

        public async Task DeleteEventAsync(Guid eventId)
        {
            var existingEvent = await eventRepository.GetEventByIdAsync(eventId);

            if (existingEvent == null)
            {
                throw new KeyNotFoundException($"Event with ID {eventId} not found");
            }

            if (existingEvent.Status == EventStatus.Ongoing && existingEvent.CurrentParticipants > 0)
            {
                throw new InvalidOperationException("Cannot delete an ongoing event with non-zero registered participants");
            } 

            await eventRepository.DeleteEventAsync(@eventId);
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            var @event = await eventRepository.GetEventByIdAsync(id);

            if (@event == null)
            {
                throw new KeyNotFoundException($"Event with ID {id} not found");
            }

            return @event;

        }

        public async Task<List<Event>> GetEventsAsync()
        {
            return await eventRepository.GetEventsAsync();
        }

        public async Task UpdateEventAsync(Guid eventId, Event @event)
        {
            var existingEvent = await eventRepository.GetEventByIdAsync(eventId);

            if (existingEvent == null)
            {
                throw new KeyNotFoundException($"Event with ID {eventId} not found");
            }

            var validationResult = await eventValidation.ValidateEventUpdateAsync(@event);

            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Event validation failed: {validationResult.ErrorMessage}");
            }

            @event.CreatedAt = existingEvent.CreatedAt;
            @event.LastUpdated = DateTime.UtcNow;

            await eventRepository.UpdateEventAsync(eventId, @event);
        }
    }
}
