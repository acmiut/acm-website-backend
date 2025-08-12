using IUT_ACM_Solution.Models;
using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        [MaxLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(255)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Student Number")]
        [MaxLength(12)]
        public string StudentNumber { get; set; } = string.Empty;

        [Required]
        public int EntryYear { get; set; }

        [Required]
        public EducationLevel EducationLevel { get; set; }

        [Required]
        [MaxLength(255)]
        public string University { get; set; } = string.Empty;

        [Display(Name = "Telegram ID")]
        [MaxLength(255)]
        public string TelegramId { get; set; } = string.Empty;
    }

}
