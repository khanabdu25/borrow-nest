namespace borrow_nest.Services;

public interface INotificationObserver
{
    Task NotifyAsync(string message, string emailAddress);
}
