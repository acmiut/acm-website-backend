using IUT_ACM_Solution.Interfaces.Repositories;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Services
{
    public class TeamManagement : ITeamManagement
    {
        private readonly ITeamRepository teamRepository;

        public TeamManagement(ITeamRepository teamRepository)
        {
            this.teamRepository = teamRepository;
        }
        public async Task CreateTeamAsync(Team team)
        {
            await teamRepository.CreateTeamAsync(team);
        }

        public async Task DeleteTeamAsync(Guid teamId)
        {
            await teamRepository.DeleteTeamAsync(teamId);
        }

        public async Task<Team> GetTeamByIdAsync(Guid teamId)
        {
            return await teamRepository.GetTeamByIdAsync(teamId);
        }

        public async Task<List<Team>> GetTeamsAsync()
        {
            return await teamRepository.GetTeamsAsync();
        }

        public async Task<List<Team>> GetTeamsByEventAsync(Guid eventId)
        {
            return await teamRepository.GetTeamsByEventAsync(eventId);
        }

        public async Task UpdateTeamAsync(Guid teamId, Team team)
        {
            await teamRepository.UpdateTeamAsync(teamId, team);
        }
    }
}
