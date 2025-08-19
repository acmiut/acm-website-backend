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
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        // Logistics
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EventType EventType { get; set; }
        public EventStatus Status { get; set; } = EventStatus.Upcoming;
        public int Capacity { get; set; }
        public int CurrentParticipants { get; set; } = 0;
        public bool IsTeamBased { get; set; } = true;
        public int MaxTeamMembers { get; set; }
        public bool RequiresRegistration { get; set; } = true;

        // Financials
        public int BasePrice { get; set; }

        public bool IsFree => BasePrice <= 0;

        // Organizers & Sponsors
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
