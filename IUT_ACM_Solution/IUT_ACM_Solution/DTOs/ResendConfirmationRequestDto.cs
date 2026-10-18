using IUT_ACM_Solution.Attributes;
using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.DTOs
{
    /// <summary>Body of POST /api/auth/resend-confirmation.</summary>
    public class ResendConfirmationRequestDto
    {
        [Required(ErrorMessage = "ایمیل دانشجویی را وارد کنید.")]
        [IutStudentEmail]
        public string Email { get; set; } = string.Empty;
    }
}
