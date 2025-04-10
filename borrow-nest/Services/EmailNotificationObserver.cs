namespace borrow_nest.Services;

public class EmailNotificationObserver : INotificationObserver
{
    private readonly IEmailSender _emailSender;
    public string UserType { get; set; }  // Used to determine whether it's the renter or owner

    public EmailNotificationObserver(IEmailSender emailSender, string userType)
    {
        _emailSender = emailSender;
        UserType = userType;
    }

    public async Task NotifyAsync(string message, string emailAddress)
    {
        // Send an email to the respective address (renter or owner)
        await _emailSender.SendEmailAsync(emailAddress, "Booking Status Update", message);
    }
}
