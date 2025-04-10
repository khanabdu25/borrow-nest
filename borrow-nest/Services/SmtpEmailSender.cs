namespace borrow_nest.Services;

using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class SmtpEmailSender : IEmailSender
{
    private readonly string _smtpServer = "smtp.yourserver.com"; // Your SMTP server address
    private readonly string _smtpUsername = "yourusername"; // Your SMTP username
    private readonly string _smtpPassword = "yourpassword"; // Your SMTP password
    private readonly int _smtpPort = 587; // SMTP Port (587 for TLS)

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using (var client = new SmtpClient(_smtpServer, _smtpPort))
        {
            client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
            client.EnableSsl = true;  // Enable SSL

            var mailMessage = new MailMessage
            {
                From = new MailAddress("noreply@yourdomain.com", "Your Booking Service"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true  // If you are sending HTML formatted emails
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
