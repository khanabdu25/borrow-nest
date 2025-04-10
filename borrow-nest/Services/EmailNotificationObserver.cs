namespace borrow_nest.Services;
using borrow_nest.Models;
using Microsoft.Extensions.Options;

public class EmailNotificationObserver : INotificationObserver
{
    private readonly IEmailSender _emailSender;
    private readonly EmailSettings _emailSettings;

    public string UserType { get; set; }

    // Modify constructor to use IOptions for EmailSettings
    public EmailNotificationObserver(IEmailSender emailSender, IOptions<EmailSettings> emailSettings)
    {
        _emailSender = emailSender;
        _emailSettings = emailSettings.Value;
    }

    public async Task NotifyAsync(string message, string emailAddress)
    {
        // Use _emailSettings.FromEmail (for example) as the sender's email address
        await _emailSender.SendEmailAsync(emailAddress, "Booking Status Update", message);
    }
}
