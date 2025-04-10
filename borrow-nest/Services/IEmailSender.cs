namespace borrow_nest.Services;

using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using borrow_nest.Models;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string message);
}

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var fromAddress = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
        var toAddress = new MailAddress(toEmail);
        var smtp = new SmtpClient
        {
            Host = _emailSettings.SmtpServer,
            Port = _emailSettings.SmtpPort,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPass),
            Timeout = 30000
        };

        using (var mailMessage = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        })
        {
            try
            {
                await smtp.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
            }
        }
    }
}

