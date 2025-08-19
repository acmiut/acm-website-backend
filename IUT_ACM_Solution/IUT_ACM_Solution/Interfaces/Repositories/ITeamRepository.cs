using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Interfaces.Repositories
{
    public interface ITeamRepository
    {
        public Task CreateTeamAsync(Team team);
        public Task<List<Team>> GetTeamsAsync();
        public Task<Team> GetTeamByIdAsync(Guid teamId);
        public Task<List<Team>> GetTeamsByEventAsync(Guid eventId);
        public Task UpdateTeamAsync(Guid teamId, Team team);
        public Task DeleteTeamAsync(Guid teamId);
    }
}
