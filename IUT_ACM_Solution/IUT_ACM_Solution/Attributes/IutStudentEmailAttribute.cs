using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IUT_ACM_Solution.Attributes
{
    /// <summary>
    /// Accepts only addresses shaped like  *@*.iut.ac.ir  (e.g. m.rezaei@ec.iut.ac.ir).
    /// At least one sub-domain label is required, so a bare "@iut.ac.ir" is rejected.
    /// null / empty values pass; combine with [Required] to reject them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class IutStudentEmailAttribute : ValidationAttribute
    {
        private static readonly Regex Pattern = new(
            @"^[a-z0-9._%+\-]+@([a-z0-9]([a-z0-9\-]*[a-z0-9])?\.)+iut\.ac\.ir\z",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(250));

        public IutStudentEmailAttribute()
        {
            ErrorMessage = "ایمیل باید به شکل name@faculty.iut.ac.ir باشد.";
        }

        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var value = email.Trim();
            if (value.Length > 254) return false;

            try
            {
                return Pattern.IsMatch(value);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public override bool IsValid(object? value)
        {
            if (value is null) return true;
            return value is string s && (s.Length == 0 || IsValidEmail(s));
        }
    }
}
