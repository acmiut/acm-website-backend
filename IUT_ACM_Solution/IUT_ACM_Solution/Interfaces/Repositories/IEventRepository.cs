using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Interfaces.Repositories
{
    public interface IEventRepository
    {
        public Task CreateEventAsync(Event @event);
        public Task<List<Event>> GetEventsAsync();
        public Task<Event> GetEventByIdAsync(Guid id);
        public Task UpdateEventAsync(Guid eventId, Event @event);
        public Task DeleteEventAsync(Guid eventId);
        public Task<bool> ExistsEventByTitleAndDateAsync(string Title, DateTime startDate, DateTime endDate);
    }
}
