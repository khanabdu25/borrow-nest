using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using borrow_nest.Models;
using borrow_nest.Services;
using Microsoft.AspNetCore.Authorization;

namespace borrow_nest.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingController : ControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private readonly BNContext _context;
        private readonly RoleCheckerService _roleChecker;

        private readonly PaymentService _paymentService;

        public BookingController(ILogger<BookingController> logger, BNContext context, RoleCheckerService roleChecker, PaymentService paymentService)
        {
            _paymentService = paymentService;
            _logger = logger;
            _context = context;
            _roleChecker = roleChecker;
        }

        [HttpPost("bookcar")]
        [Authorize(Roles = "USER")]
        public async Task<IActionResult> BookCar([FromBody] BookCarRequest request)
        {
            var car = await _context.CarListings
                            .FirstOrDefaultAsync(c => c.Id == request.ListingId && c.Status == CarListing.CarStatus.Available);

            if (car == null)
            {
                return NotFound("Car not available.");
            }

            var startDateUtc = request.StartDate.ToUniversalTime();
            var endDateUtc = request.EndDate.ToUniversalTime();

            // Check availability for the car
            if (car.ReservedStartDate.HasValue && car.ReservedEndDate.HasValue &&
                ((startDateUtc < car.ReservedEndDate && startDateUtc >= car.ReservedStartDate) ||
                (endDateUtc <= car.ReservedEndDate && endDateUtc > car.ReservedStartDate)))
            {
                return BadRequest("The car is already booked for the selected dates.");
            }

            BNUser renter = await _roleChecker.GetCurrentUserAsync();
            if (renter == null)
            {
                return Unauthorized("User must be logged in to book a car.");
            }

            // Assume Owner info is fetched somehow
            BNUser owner = await _context.BNUsers.FirstOrDefaultAsync(u => u.Id == request.OwnerId);

            if (owner == renter)
            {
                return BadRequest("You cannot book your own car.");
            }

            // Create booking
            var booking = new Booking
            {
                Car = car,
                Renter = renter,
                Owner = owner,
                StartDate = startDateUtc,
                EndDate = endDateUtc,
                TotalPrice = (decimal)(endDateUtc - startDateUtc).TotalDays * car.PricePerDay,
                Status = Booking.BookingStatus.Pending
            };

            car.Status = CarListing.CarStatus.Booked;
            car.ReservedStartDate = startDateUtc;
            car.ReservedEndDate = endDateUtc;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var fullBooking = await _context.Bookings
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.Id == booking.Id);

            // Check if the booking was found and return it
            if (fullBooking != null)
            {
                return Ok(new
                {
                    message = "Booking confirmed",
                    booking = fullBooking
                });
            }
            else
            {
                return NotFound(new { message = "Booking not found" });
            }
        }

        // New endpoint for initiating payment and confirming booking
        [HttpPost("initiate")]
        [Authorize(Roles = "USER")]
        public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequest paymentRequest)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == paymentRequest.BookingId);

            if (booking == null)
            {
                return NotFound(new { message = "Booking not found." });
            }
            if (booking.Status != Booking.BookingStatus.Pending)
            {
                return BadRequest(new { message = "Booking is not in a valid state for payment." });
            }
            // Process payment (mocked payment gateway)
            var paymentSuccess = await _paymentService.ProcessPaymentAsync(paymentRequest);

            if (!paymentSuccess)
            {
                return BadRequest(new { message = "Payment failed. Please try again." });
            }

            var bookingConfirmed = false;

            // Confirm the booking once payment is successful


            booking.Status = Booking.BookingStatus.Confirmed;
            await _context.SaveChangesAsync();
            bookingConfirmed = true;


            if (!bookingConfirmed)
            {
                return NotFound(new { message = "Booking not found or could not be confirmed." });
            }

            // Return success response
            return Ok(new { message = "Payment processed and booking confirmed." });
        }

        [HttpGet("mine")]
        [Authorize(Roles = "USER")]
        public async Task<IActionResult> GetMyBookings()
        {
            // Get the currently logged-in user (either as renter or owner)
            BNUser currentUser = await _roleChecker.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("User must be logged in to view bookings.");
            }

            // Fetch bookings where the current user is either the renter or the owner
            var bookings = await _context.Bookings
                .Where(b => b.Renter.Id == currentUser.Id
                        && b.Status != Booking.BookingStatus.Completed
                        && b.Status != Booking.BookingStatus.Canceled)
                .Include(b => b.Car)
                .Include(b => b.Renter)
                .Include(b => b.Owner)
                .ToListAsync();

            if (bookings == null || bookings.Count == 0)
            {
                return NotFound(new { message = "No bookings found for this user." });
            }

            return Ok(bookings);
        }

        [HttpPost("returncar")]
        [Authorize(Roles = "USER")]
        public async Task<IActionResult> ReturnCar([FromBody] ReturnCarRequest request)
        {
            try
            {
                // Get the booking based on the BookingId
                var booking = await _context.Bookings
                                             .Include(b => b.Car)
                                             .FirstOrDefaultAsync(b => b.Id == request.BookingId);

                if (booking == null)
                {
                    return NotFound("Booking not found.");
                }

                // Check if the car is already returned or not booked
                if (booking.Status == Booking.BookingStatus.Completed || booking.Status == Booking.BookingStatus.Canceled)
                {
                    return BadRequest("This booking has already been completed or canceled.");
                }

                // Mark the booking as completed (or you can cancel it)
                booking.Status = Booking.BookingStatus.Completed;

                // Make the car listing available again
                var car = booking.Car;
                car.Status = CarListing.CarStatus.Available;
                car.ReservedStartDate = null;
                car.ReservedEndDate = null;

                // Update the booking and car listing in the database
                _context.Bookings.Update(booking);
                _context.CarListings.Update(car);
                await _context.SaveChangesAsync();

                return Ok("Car has been successfully returned and is now available.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while returning the car.");
                return StatusCode(500, "Internal server error while processing return.");
            }
        }

    }
}
