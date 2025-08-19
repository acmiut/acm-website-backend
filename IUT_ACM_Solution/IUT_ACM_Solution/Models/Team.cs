using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.Models
{
    public class Team
    {
        public Guid Id { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public List<string> TeamMemberEmails { get; set; } = new();

        public bool HasPaid { get; set; } = false;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public Guid EventId { get; set; }
        public Event? Event { get; set; }
    }

}

