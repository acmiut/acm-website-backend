using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IUT_ACM_Solution.Models
{
    public enum EducationLevel
    {
        Undergraduate = 0,
        Master = 1,
        Doctoral = 2
    }
    public class User : IdentityUser
    {
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

        [Display(Name = "Student Number")]
        [MaxLength(12)]
        public string StudentNumber {  get; set; } = string.Empty;

        public int EntryYear { get; set; }

        public EducationLevel EducationLevel { get; set; } = EducationLevel.Undergraduate;

        [MaxLength(255)]
        public string? University { get; set; } = string.Empty;

        [Display(Name = "Telegram ID")]
        [MaxLength(255)]
        public string? TelegramId { get; set; } = string.Empty;

        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
