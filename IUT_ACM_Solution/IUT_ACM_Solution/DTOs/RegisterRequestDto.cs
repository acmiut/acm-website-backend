using IUT_ACM_Solution.Attributes;
using IUT_ACM_Solution.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace IUT_ACM_Solution.DTOs
{
    /// <summary>Body of POST /api/auth/register (sent by the React sign-up page).</summary>
    public class RegisterRequestDto : IValidatableObject
    {
        public const int MinEntryYear = 1380;

        [Required(ErrorMessage = "نام را وارد کنید.")]
        [StringLength(255, ErrorMessage = "نام نباید بیشتر از ۲۵۵ کاراکتر باشد.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام خانوادگی را وارد کنید.")]
        [StringLength(255, ErrorMessage = "نام خانوادگی نباید بیشتر از ۲۵۵ کاراکتر باشد.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ایمیل دانشجویی را وارد کنید.")]
        [StringLength(254, ErrorMessage = "ایمیل بیش از حد طولانی است.")]
        [IutStudentEmail]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره تماس را وارد کنید.")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره تماس باید مانند 09123456789 باشد.")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره دانشجویی را وارد کنید.")]
        [RegularExpression(@"^\d{6,12}$", ErrorMessage = "شماره دانشجویی باید بین ۶ تا ۱۲ رقم باشد.")]
        public string StudentNumber { get; set; } = string.Empty;

        /// <summary>Solar-hijri (Persian) year, e.g. 1403.</summary>
        public int EntryYear { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EducationLevel EducationLevel { get; set; } = EducationLevel.Undergraduate;

        [RegularExpression(@"^@?[A-Za-z][A-Za-z0-9_]{4,31}$", ErrorMessage = "آیدی تلگرام معتبر نیست.")]
        public string? TelegramId { get; set; }

        [Required(ErrorMessage = "رمز عبور را وارد کنید.")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "رمز عبور باید بین ۸ تا ۱۲۸ کاراکتر باشد.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var currentYear = new PersianCalendar().GetYear(DateTime.UtcNow);
            if (EntryYear < MinEntryYear || EntryYear > currentYear)
            {
                yield return new ValidationResult(
                    $"سال ورود باید (به تقویم شمسی) بین {MinEntryYear} و {currentYear} باشد.",
                    new[] { nameof(EntryYear) });
            }

            if (!Enum.IsDefined(typeof(EducationLevel), EducationLevel))
            {
                yield return new ValidationResult(
                    "مقطع تحصیلی معتبر نیست.",
                    new[] { nameof(EducationLevel) });
            }
        }
    }
}
