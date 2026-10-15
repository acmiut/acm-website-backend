using IUT_ACM_Solution.Models;

namespace IUT_ACM_Solution.Interfaces.Validations
{
    public interface IEventValidation
    {
        IValidationResult ValidateEvent(Event @event);
        Task<IValidationResult> ValidateEventCreationAsync(Event @event);
        Task<IValidationResult> ValidateEventUpdateAsync(Event @event);
    }
}
