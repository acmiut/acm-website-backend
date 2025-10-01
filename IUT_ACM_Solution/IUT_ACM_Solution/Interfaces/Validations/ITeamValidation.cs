using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Interfaces.Validations
{
    public interface ITeamValidation
    {
        Task<IValidationResult> ValidateTeam(Team team);
        Task<IValidationResult> ValidateTeamCreationAsync(Team team);
        Task<IValidationResult> ValidateTeamUpdateAsync(Team team);
    }
}
