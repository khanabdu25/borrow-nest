namespace borrow_nest.Models
{
    using borrow_nest.Services;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Booking
    {
        public int Id { get; set; }
        public CarListing Car { get; set; }
        public BNUser Renter { get; set; }
        public BNUser Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }

        [NotMapped]
        public List<INotificationObserver> Observers { get; set; } = new List<INotificationObserver>();

        public void RegisterObservers(INotificationObserver renterObserver, INotificationObserver ownerObserver)
        {
            Observers.Add(renterObserver);
            Observers.Add(ownerObserver);
        }

        // Notify all registered observers (renter and owner)
        public async Task NotifyObservers(string message)
        {
            foreach (var observer in Observers)
            {
                // Use different emails for renter and owner
                if (observer is EmailNotificationObserver emailObserver)
                {
                    if (emailObserver.UserType == "Renter")
                    {
                        await emailObserver.NotifyAsync(message, this.Renter.Email);
                    }
                    else if (emailObserver.UserType == "Owner")
                    {
                        await emailObserver.NotifyAsync(message, this.Owner.Email);
                    }
                }
            }
        }

        // Change status and notify the observers
        public async Task ChangeStatus(BookingStatus newStatus)
        {
            await NotifyObservers($"Booking status changed to: {newStatus}");
        }


        public enum BookingStatus
        {
            Pending,
            Confirmed,
            Completed,
            Canceled
        }
    }
}
