using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Services
{
    public class EventManagement : IEventManagement
    {
        public Task CreateEventAsync(Event @event)
        {
            throw new NotImplementedException();
        }

        public Task DeleteEventAsync(Guid eventId)
        {
            throw new NotImplementedException();
        }

        public Task<Event> GetEventByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Event>> GetEventsAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateEventAsync(Guid eventId, Event @event)
        {
            throw new NotImplementedException();
        }
    }
}
