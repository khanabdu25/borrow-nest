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

                        //             string body = string.Empty;

                        //             if (this.Status == BookingStatus.Pending)
                        //             {
                        //                 body = $@"
                        //             <h3>Your reservation for the {this.Car.Year} {this.Car.Model} has been placed!</h3>
                        //             <p>Head over to <strong>{this.Car.PickUpLocation}</strong> to pick it up.</p>
                        //         ";
                        //             }
                        //             else if (this.Status == BookingStatus.Confirmed)
                        //             {
                        //                 body = $@"
                        //     <h3>Your rental for the {this.Car.Year} {this.Car.Model} has started, enjoy!</h3>
                        //     <p>You have until <strong>{this.Car.ReservedEndDate?.ToString("MMMM dd, yyyy")}</strong> to return it.</p>
                        // ";
                        //             }
                        //             else if (this.Status == BookingStatus.Completed)
                        //             {
                        //                 body = $@"
                        //     <h3>You have successfully returned the {this.Car.Year} {this.Car.Model}.</h3>
                        //     <p>We hope to serve you again!</p>
                        // ";
                        //             }

                        //             await emailObserver.NotifyAsync(body, this.Renter.Email);
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
