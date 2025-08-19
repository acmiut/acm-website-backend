using IUT_ACM_Solution.Interfaces.Repositories;
using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        public Task CreateTeamAsync(Team team)
        {
            throw new NotImplementedException();
        }

        public Task DeleteTeamAsync(Guid teamId)
        {
            throw new NotImplementedException();
        }

        public Task<Team> GetTeamByIdAsync(Guid teamId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Team>> GetTeamsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Team>> GetTeamsByEventAsync(Guid eventId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTeamAsync(Guid teamId, Team team)
        {
            throw new NotImplementedException();
        }
    }
}
