using IUT_ACM_Solution.Interfaces.Validations;
using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.Services.Validations
{
    public class ValidationResult : IValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool isValid, string errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Valid()
        {
            return new ValidationResult(true, string.Empty);
        }

        public static ValidationResult Invalid(string errorMessage)
        {
            return new ValidationResult(false, errorMessage);
        }
    }
}
