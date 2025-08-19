using IUT_ACM_Solution.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using IUT_ACM_Solution.Interfaces.Services;

namespace IUT_ACM_Solution.Services
{
    public class EmailService : IEmailService
    {
        private readonly IOptions<SmtpSetting> smtpSetting;

        public EmailService(IOptions<SmtpSetting> smtpSetting)
        {
            this.smtpSetting = smtpSetting;
        }
        public async Task SendAsync(string to, string subject, string body)
        {
            var message = new MailMessage(
                smtpSetting.Value.SenderEmail,
                to,
                subject,
                body);

            using (var emailClient = new SmtpClient(smtpSetting.Value.Host, smtpSetting.Value.Port))
            {
                emailClient.Credentials = new NetworkCredential(
                    smtpSetting.Value.Username,
                    smtpSetting.Value.Password
                    );

                emailClient.EnableSsl = true;

                await emailClient.SendMailAsync(message);
            }
        }
    }
}
