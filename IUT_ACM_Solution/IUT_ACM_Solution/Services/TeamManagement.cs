using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Services
{
    public class TeamManagement : ITeamManagement
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
