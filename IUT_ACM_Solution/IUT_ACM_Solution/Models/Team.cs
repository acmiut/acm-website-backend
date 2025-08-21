using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.Models
{
    public class Team
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        public List<string> TeamMemberEmails { get; set; } = new();

        public bool HasPaid { get; set; } = false;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public int FeeToPay;

        // Foreign Key
        [Required]
        public Guid EventId { get; set; }

        // Navigation Property
        public virtual Event? Event { get; set; }

        // Team Leader (first email in the list by convention, or separate property)
        public string TeamLeaderEmail => TeamMemberEmails.FirstOrDefault() ?? string.Empty;

        // Validation
        public bool HasValidEmails => TeamMemberEmails.All(email =>
            !string.IsNullOrEmpty(email) && email.Contains("@"));
    }

}

