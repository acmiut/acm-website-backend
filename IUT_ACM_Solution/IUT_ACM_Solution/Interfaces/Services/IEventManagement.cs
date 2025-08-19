using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Interfaces.Services
{
    public interface IEventManagement
    {
        public Task CreateEventAsync(Event @event);
        public Task<List<Event>> GetEventsAsync();
        public Task<Event> GetEventByIdAsync(Guid id);
        public Task UpdateEventAsync(Guid eventId, Event @event);
        public Task DeleteEventAsync(Guid eventId);
    }
}
