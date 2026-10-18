using IUT_ACM_Solution.DTOs;

namespace IUT_ACM_Solution.Interfaces.Services
{
    public interface IRegistrationService
    {
        /// <summary>Validates, stores a PendingRegistration and emails the confirmation link. Does NOT create a User.</summary>
        Task<RegistrationOutcome> StartRegistrationAsync(RegisterRequestDto dto, CancellationToken ct = default);

        /// <summary>Sends a fresh confirmation link for an existing, unexpired pending sign-up.</summary>
        Task<RegistrationOutcome> ResendConfirmationAsync(string email, CancellationToken ct = default);

        /// <summary>Consumes the token: creates the real User from the pending data and deletes the pending row.</summary>
        Task<RegistrationOutcome> ConfirmEmailAsync(string token, CancellationToken ct = default);
    }
}
