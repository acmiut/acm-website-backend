namespace IUT_ACM_Solution.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string from, string to, string subject, string body);
    }
}
