using IUT_ACM_Solution.Interfaces.Repositories;
using IUT_ACM_Solution.Interfaces.Services;
using IUT_ACM_Solution.Interfaces.Validations;
using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Services
{
    public class TeamManagement : ITeamManagement
    {
        private readonly ITeamRepository teamRepository;
        private readonly ITeamValidation teamValidation;

        public TeamManagement(ITeamRepository teamRepository, ITeamValidation teamValidation)
        {
            this.teamRepository = teamRepository;
            this.teamValidation = teamValidation;
        }

        public async Task CreateTeamAsync(Team team)
        {
            //var validationResult = await teamValidation.ValidateTeamCreationAsync(team);
            //if (!validationResult.IsValid)
            //{
            //    throw new InvalidOperationException(validationResult.ErrorMessage);
            //}


            await teamRepository.CreateTeamAsync(team);
        }

        public async Task UpdateTeamAsync(Guid teamId, Team team)
        {
            /*if (team.Id != Guid.Empty && team.Id != teamId)
            {
                throw new InvalidOperationException("Team ID mismatch between route and payload");
            }

            var validationResult = await teamValidation.ValidateTeamUpdateAsync(team);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(validationResult.ErrorMessage);
            }*/

            await teamRepository.UpdateTeamAsync(teamId, team);
        }

        public async Task DeleteTeamAsync(Guid teamId)
        {
            var existingTeam = await teamRepository.GetTeamByIdAsync(teamId);
            if (existingTeam == null)
            {
                throw new KeyNotFoundException("Team not found");
            }

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
    }
}
