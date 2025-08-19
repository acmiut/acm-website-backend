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


        [Required]
        [Phone]
        [Display(Name = "Contact Number")]
        [MaxLength(20)]
        public string ContactNumber { get; set; } = string.Empty;
    }

}
