namespace IUT_ACM_Solution.Models
{
    public class UserEvent
    {
        public string UserId { get; set; } = string.Empty;
        public Guid EventId { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public bool HasPaid { get; set; } = false;

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Event? Event { get; set; }
    }
}
