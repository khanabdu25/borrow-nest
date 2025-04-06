using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using borrow_nest.Models;
using borrow_nest.Services;

namespace borrow_nest.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingController : ControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private readonly BNContext _context;
        private readonly RoleCheckerService _roleChecker;

        public BookingController(ILogger<BookingController> logger, BNContext context, RoleCheckerService roleChecker)
        {
            _logger = logger;
            _context = context;
            _roleChecker = roleChecker;
        }

        [HttpPost("bookcar")]
        public async Task<IActionResult> BookCar([FromBody] BookCarRequest request)
        {
            var car = await _context.CarListings
                                    .FirstOrDefaultAsync(c => c.Model == request.CarModel && c.Status == CarListing.CarStatus.Available);

            if (car == null)
            {
                return NotFound("Car not available.");
            }

            // Check availability for the car
            if (car.ReservedStartDate.HasValue && car.ReservedEndDate.HasValue &&
                ((request.StartDate < car.ReservedEndDate && request.StartDate >= car.ReservedStartDate) ||
                (request.EndDate <= car.ReservedEndDate && request.EndDate > car.ReservedStartDate)))
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

            // Create booking
            var booking = new Booking
            {
                Car = car,
                Renter = renter,
                Owner = owner,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalPrice = (decimal)(request.EndDate - request.StartDate).TotalDays * car.PricePerDay,
                Status = Booking.BookingStatus.Pending
            };

            car.Status = CarListing.CarStatus.Booked;
            car.ReservedStartDate = request.StartDate;
            car.ReservedEndDate = request.EndDate;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking confirmed", bookingId = booking.Id });
        }
    }
}
