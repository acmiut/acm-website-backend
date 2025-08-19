using IUT_ACM_Solution.Data;
using IUT_ACM_Solution.Interfaces.Repositories;
using IUT_ACM_Solution.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IUT_ACM_Solution.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext dbContext;

        public TeamRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task CreateTeamAsync(Team team)
        {
            try
            {
                if (!team.HasValidEmails) return;

                await dbContext.Teams.AddAsync(team);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding team", ex);
            }
        }

        public async Task DeleteTeamAsync(Guid teamId)
        {
            try
            {
                var team = await dbContext.Teams.FindAsync(teamId);
                if (team == null) return;

                dbContext.Teams.Remove(team);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting team", ex);
            }
        }

        public async Task<Team> GetTeamByIdAsync(Guid teamId)
        {
            try
            {
                return await dbContext.Teams.FindAsync(teamId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting team by id", ex);
            }
        }

        public async Task<List<Team>> GetTeamsAsync()
        {
            try
            {
                return await dbContext.Teams.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting teams", ex);
            }
        }

        public async Task<List<Team>> GetTeamsByEventAsync(Guid eventId)
        {
            try
            {
                return await dbContext.Teams.Where(x => x.EventId == eventId).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting team by event", ex);
            }
        }

        public async Task UpdateTeamAsync(Guid teamId, Team team)
        {
            try
            {
                if (team == null) return;
                if (team.Id != teamId) return;
                if (!team.HasValidEmails) return;

                var _team = await dbContext.Teams.FindAsync(teamId);
                if (_team == null) return;

                _team.TeamName = team.TeamName;
                _team.TeamMemberEmails = team.TeamMemberEmails;
                _team.HasPaid = team.HasPaid;
                _team.RegisteredAt = team.RegisteredAt;

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating team", ex);
            }
        }
    }
}
