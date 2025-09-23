namespace IUT_ACM_Solution.DTOs
{
    public class TeamRegisterationViewModel
    {
        public Guid EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public int Fee { get; set; }
        public TeamDto Team { get; set; } = new();
    }
}
