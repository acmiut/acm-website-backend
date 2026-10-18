namespace IUT_ACM_Solution.DTOs
{
    public enum RegistrationStatus
    {
        Success,
        EmailAlreadyRegistered,
        WeakPassword,
        Cooldown,
        EmailSendFailed,
        InvalidOrExpiredToken,
        NotFound,
        Failed
    }

    /// <summary>
    /// Result of a registration step.
    /// WaitSeconds: on Success = seconds until a resend is allowed;
    /// on Cooldown = seconds the caller still has to wait.
    /// </summary>
    public sealed record RegistrationOutcome(RegistrationStatus Status, int? WaitSeconds = null)
    {
        public static RegistrationOutcome Ok(int resendAfterSeconds) => new(RegistrationStatus.Success, resendAfterSeconds);
        public static RegistrationOutcome Fail(RegistrationStatus status) => new(status);
        public static RegistrationOutcome Wait(int seconds) => new(RegistrationStatus.Cooldown, seconds);
    }
}
