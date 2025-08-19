using IUT_ACM_Solution.Data;
using IUT_ACM_Solution.Interfaces.Repositories;
using IUT_ACM_Solution.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IUT_ACM_Solution.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext dbContext;

        public EventRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task CreateEventAsync(Event @event)
        {
            try
            {
                if (!@event.IsValidDateRange) return;
                await dbContext.Events.AddAsync(@event);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding event", ex);
            }
        }

        public async Task DeleteEventAsync(Guid eventId)
        {
            try
            {
                var @event = await dbContext.Events.FindAsync(eventId);
                if (@event == null) return;

                dbContext.Events.Remove(@event);
                await dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting event", ex);
            }
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            try
            {
                return await dbContext.Events.FindAsync(id);

            }
            catch (Exception ex)
            {
                throw new Exception("Error getting event by id", ex);
            }
        }

        public async Task<List<Event>> GetEventsAsync()
        {
            try
            {
                return await dbContext.Events.ToListAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error getting events", ex);
            }
        }

        public async Task UpdateEventAsync(Guid eventId, Event @event)
        {
            try
            {
                if (@event == null) return;
                if (@event.Id != eventId) return;
                if (!@event.IsValidDateRange) return;

                var _event = await dbContext.Events.FindAsync(eventId);
                if (_event == null) return;

                _event.Title = @event.Title;
                _event.Description = @event.Description;
                _event.ImageUrl = @event.ImageUrl;
                _event.Location = @event.Location;
                _event.StartDate = @event.StartDate;
                _event.EndDate = @event.EndDate;
                _event.EventType = @event.EventType;
                _event.Status = @event.Status;
                _event.Capacity = @event.Capacity;
                _event.CurrentParticipants = @event.CurrentParticipants;
                _event.IsTeamBased = @event.IsTeamBased;
                _event.MaxTeamMembers = @event.MaxTeamMembers;
                _event.RequiresRegistration = @event.RequiresRegistration;
                _event.BasePrice = @event.BasePrice;
                _event.OrganizedBy = @event.OrganizedBy;
                _event.Sponsors = @event.Sponsors;

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating event", ex);
            }
        }
    }
}
