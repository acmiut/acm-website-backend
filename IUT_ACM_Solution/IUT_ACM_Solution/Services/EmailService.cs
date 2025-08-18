using IUT_ACM_Solution.Interfaces;
using IUT_ACM_Solution.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace IUT_ACM_Solution.Services
{
    public class EmailService : IEmailService
    {
        private readonly IOptions<SmtpSetting> smtpSetting;

        public EmailService(IOptions<SmtpSetting> smtpSetting)
        {
            this.smtpSetting = smtpSetting;
        }
        public async Task SendAsync(string from, string to, string subject, string body)
        {
            var message = new MailMessage(from,
                to,
                subject,
                body);

            using (var emailClient = new SmtpClient(smtpSetting.Value.Host, smtpSetting.Value.Port))
            {
                emailClient.Credentials = new NetworkCredential(
                    smtpSetting.Value.Username,
                    smtpSetting.Value.Password
                    );

                await emailClient.SendMailAsync(message);
            }
        }
    }
}
