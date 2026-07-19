using System.Net;
using System.Net.Mail;

namespace ECommerceWeb.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Send(string to, string subject, string body)
        {
            var smtp = _configuration.GetSection("Smtp");

            SmtpClient client = new SmtpClient(smtp["Host"])
            {
                Port = int.Parse(smtp["Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    smtp["Email"],
                    smtp["Password"])
            };

            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(smtp["Email"]);

            mail.To.Add(to);

            mail.Subject = subject;

            mail.Body = body;

            client.Send(mail);
        }
    }
}