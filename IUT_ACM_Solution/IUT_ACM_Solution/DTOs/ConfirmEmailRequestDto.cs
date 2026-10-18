using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.DTOs
{
    /// <summary>Body of POST /api/auth/confirm-email.</summary>
    public class ConfirmEmailRequestDto
    {
        [Required]
        [StringLength(256)]
        public string Token { get; set; } = string.Empty;
    }
}
