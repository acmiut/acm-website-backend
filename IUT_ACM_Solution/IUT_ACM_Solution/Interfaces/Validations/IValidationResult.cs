namespace IUT_ACM_Solution.Interfaces.Validations
{
    public interface IValidationResult
    {
        bool IsValid { get; }
        string ErrorMessage { get; }
    }
}
