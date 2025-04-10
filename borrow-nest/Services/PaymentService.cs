namespace borrow_nest.Services;
using borrow_nest.Models;
using Microsoft.EntityFrameworkCore;
public class PaymentService(BNContext context, RoleCheckerService roleChecker)
{
    private readonly BNContext _context = context;

    private readonly RoleCheckerService _roleChecker = roleChecker;

    public async Task<bool> ProcessPaymentAsync(PaymentRequest paymentRequest)
    {
        //request has Amount, BookingId
        // Here, simulate communication with the payment gateway (mocked).
        Console.WriteLine($"Processing payment of {paymentRequest.Amount} for booking {paymentRequest.BookingId}");

        var loggedInUser = await _roleChecker.GetCurrentUserAsync() ?? throw new UnauthorizedAccessException("User must be logged in to process payment.");
        // find the booking by id
        var fullBooking = await _context.Bookings
            .Include(b => b.Car)
            .Include(b => b.Renter)
            .Include(b => b.Owner)
            .FirstOrDefaultAsync(b => b.Id == paymentRequest.BookingId) ?? throw new ArgumentException("Booking not found.");
        // Let's assume the payment is always successful (mocked).
        var payment = new Payment
        {
            Recipient = fullBooking.Owner,
            Amount = paymentRequest.Amount,
            Booking = fullBooking,
            Sender = loggedInUser,

        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return true;  // Mock success
    }
}

