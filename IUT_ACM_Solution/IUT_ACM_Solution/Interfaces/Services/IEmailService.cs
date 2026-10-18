namespace IUT_ACM_Solution.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body, bool isHtml = false);
    }
}
