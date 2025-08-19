using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.DTOs
{
    public class TeamDto
    {
        public Guid? Id { get; set; } // Optional for creation, useful for updates

        [Required(ErrorMessage = "Team name is required.")]
        [StringLength(100, ErrorMessage = "Team name cannot exceed 100 characters.")]
        public string TeamName { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one team member email is required.")]
        [MinLength(1, ErrorMessage = "Team must have at least one member.")]
        public List<string> TeamMemberEmails { get; set; } = new();

        public bool HasPaid { get; set; } = false;

        public DateTime? RegisteredAt { get; set; } 

        [Required(ErrorMessage = "Event ID is required.")]
        public Guid EventId { get; set; }
    }
}
