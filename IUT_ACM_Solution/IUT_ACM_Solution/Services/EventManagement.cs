using IUT_ACM_Solution.Interfaces.Repositories;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Services
{
    public class EventManagement : IEventManagement
    {
        private readonly IEventRepository eventRepository;

        public EventManagement(IEventRepository eventRepository)
        {
            this.eventRepository = eventRepository;
        }
        public async Task CreateEventAsync(Event @event)
        {
            await eventRepository.CreateEventAsync(@event);
        }

        public async Task DeleteEventAsync(Guid eventId)
        {
            await eventRepository.DeleteEventAsync(@eventId);
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            return await eventRepository.GetEventByIdAsync(id);
        }

        public async Task<List<Event>> GetEventsAsync()
        {
            return await eventRepository.GetEventsAsync();
        }

        public async Task UpdateEventAsync(Guid eventId, Event @event)
        {
            await eventRepository.UpdateEventAsync(eventId, @event);
        }
    }
}
