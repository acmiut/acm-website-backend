using IUT_ACM_Solution.Attributes;
using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.Models
{
    public enum EventType
    {
        Online = 0,
        Offline = 1,
        InPerson = 2
    }

    public enum EventStatus
    {
        Upcoming,
        Ongoing,
        Completed,
        Cancelled
    }

    public class Event
    {
        public Guid Id { get; set; }

        // Core Info
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        // Logistics
        [StringLength(300)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public EventType EventType { get; set; }
        public EventStatus Status { get; set; } = EventStatus.Upcoming;

        [Range(1, 10000)]
        public int Capacity { get; set; }

        [Range(0, int.MaxValue)]
        public int CurrentParticipants { get; set; } = 0;

        public bool IsTeamBased { get; set; } = true;

        [Range(1, 50)]
        public int MaxTeamMembers { get; set; } = 5;

        public bool RequiresRegistration { get; set; } = true;

        // Financials
        [Range(0, int.MaxValue)]
        public int Fee { get; set; }

        public bool IsFree => Fee <= 0;

        // Organizers & Sponsors
        [StringLength(200)]
        public string OrganizedBy { get; set; } = "ACM Chapter";

        public List<string> Sponsors { get; set; } = new();

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<UserEvent> UserEvents { get; set; } = new List<UserEvent>();
        public virtual ICollection<Team> ParticipatingTeams { get; set; } = new List<Team>();

        // Custom validation
        public bool IsValidDateRange => EndDate > StartDate;
        public bool HasCapacityLeft => CurrentParticipants < Capacity;
    }

}
