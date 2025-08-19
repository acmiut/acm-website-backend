using IUT_ACM_Solution.Attributes;
using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.Models
{
    public enum EventType
    {
        Online = 0,
        Offline = 1,
        Hybrid = 2
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
        public int Id { get; set; }

        // Core Info
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Url]
        public string ImageUrl { get; set; } = string.Empty;

        // Logistics
        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DateGreaterThan(nameof(StartDate), ErrorMessage = "End date must be after start date.")]
        public DateTime EndDate { get; set; }

        [Required]
        public EventType EventType { get; set; }

        [Required]
        public EventStatus Status { get; set; } = EventStatus.Upcoming;

        // Participation
        [Range(1, 1000)]
        public int Capacity { get; set; }

        [Range(1, 10)]
        public int MaxTeamMembers { get; set; }

        public bool IsTeamBased { get; set; } = true;

        public bool RequiresRegistration { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int CurrentParticipants { get; set; } = 0;

        // Financials
        [Range(0, 10000)]
        public float BasePrice { get; set; }

        public bool IsFree => BasePrice <= 0;

        // Organizers & Sponsors
        [Required]
        [StringLength(100)]
        public string OrganizedBy { get; set; } = "ACM Chapter";

        public List<string> Sponsors { get; set; } = new();

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public List<User> RegisteredUsers { get; set; } = new();
        //public List<Team> ParticipatingTeams { get; set; } = new();
    }

}
