using IUT_ACM_Solution.Interfaces;
using Microsoft.Extensions.Options;

namespace IUT_ACM_Solution.Services
{
    public class EmailService : IEmailService
    {
        public Task SendAsync(string from, string to, string subject, string body)
        {
            throw new NotImplementedException();
        }
    }
}
