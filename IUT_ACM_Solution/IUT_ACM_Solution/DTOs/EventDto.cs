using IUT_ACM_Solution.Models;
using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.DTOs
{
    public class EventDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public IFormFile? ImageFile { get; set; }

        [StringLength(300)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public EventType EventType { get; set; } = EventType.Online;
        public EventStatus Status { get; set; } = EventStatus.Upcoming;

        [Range(1, 10000)]
        public int Capacity { get; set; }

        public bool IsTeamBased { get; set; } = true;

        [Range(1, 50)]
        public int MaxTeamMembers { get; set; } = 5;

        public bool RequiresRegistration { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int Fee { get; set; }

        [StringLength(200)]
        public string OrganizedBy { get; set; } = "ACM Chapter";
    }
}
