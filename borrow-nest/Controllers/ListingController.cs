using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using borrow_nest.Models;
using borrow_nest.Services;

namespace borrow_nest.Controllers;

[ApiController]
[Route("listings")]
public class ListingController : ControllerBase
{
    private readonly ILogger<ListingController> _logger;

    private readonly BNContext _context;

    private readonly RoleCheckerService _roleChecker;

    public ListingController(ILogger<ListingController> logger, BNContext context, RoleCheckerService roleChecker)
    {
        _logger = logger;
        _context = context;
        _roleChecker = roleChecker;
    }

    [HttpGet("all")]
    [Authorize(Roles = "USER")]
    public async Task<IActionResult> GetAll()
    {
        var listings = await _context.Listings
                                 .Include(l => l.Product)
                                 .ToListAsync();
        return Ok(listings);
    }

    [HttpGet("cars")]
    [Authorize(Roles = "USER")]
    public async Task<IActionResult> GetAvailableCars()
    {
        var listings = await _context.CarListings
                                     .Where(l => l.Status == CarListing.CarStatus.Available)
                                     .Include(l => l.Seller)
                                     .ToListAsync();

        return Ok(listings);
    }

    [HttpGet("cars/{id}")]
    [Authorize(Roles = "USER")]
    public async Task<IActionResult> GetCarById(int id)
    {
        var carListing = await _context.CarListings
                                       .Where(l => l.Id == id)
                                       .Include(l => l.Seller)
                                       .FirstOrDefaultAsync();

        if (carListing == null)
        {
            return NotFound($"Car listing with ID {id} not found.");
        }

        return Ok(carListing);
    }


    [HttpPost("create")]
    [Authorize(Roles = "USER")]
    public async Task<IActionResult> CreateListing([FromBody] CreateListingRequest request)
    {
        try
        {
            BNUser user = await _roleChecker.GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized("User must be logged in to create a listing.");
            }

            Product product = new Product
            {
                Status = request.Product.Status,
                HourlyRate = request.Product.HourlyRate,
                DailyRate = request.Product.DailyRate,
                WeeklyRate = request.Product.WeeklyRate,
                MonthlyRate = request.Product.MonthlyRate
            };


            Listing listing = new Listing
            {
                Title = request.Title,
                Body = request.Body,
                BNUser = user,
                Product = product,
                Status = ListingStatus.AVAILABLE
            };

            _context.Products.Add(product);
            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Listing created successfully", listingId = listing.ID });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create listing");
            return StatusCode(500, "Internal server error while creating listing");
        }
    }

    [HttpPost("new")]
    [Authorize(Roles = "USER")]
    public async Task<IActionResult> CreateCarListing([FromBody] CreateCarListingRequest request)
    {
        try
        {
            // Ensure the user is logged in
            var user = await _roleChecker.GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized("User must be logged in to create a car listing.");
            }

            // Use the Builder pattern to create the car listing and set the seller to the logged-in user
            var carListing = new CarListing.Builder()
                .SetModel(request.Model)
                .SetYear(request.Year)
                .SetMileage(request.Mileage)
                .SetLocation(request.Location)
                .SetPricePerDay(request.PricePerDay)
                .SetAvailability(request.Availability)
                .SetPickUpLocation(request.PickUpLocation)
                .SetSeller(user)
                .Build();

            // Add the car listing to the database
            _context.CarListings.Add(carListing);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Car listing created successfully", listingId = carListing.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create car listing");
            return StatusCode(500, "Internal server error while creating car listing");
        }
    }


    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] PaymentRequest request)
    {
        var listing = await _context.Listings
                                    .Include(l => l.BNUser)
                                    .FirstOrDefaultAsync(l => l.ID == request.ListingId);

        if (listing == null)
        {
            return NotFound("Listing not found.");
        }

        if (listing.Status == ListingStatus.CLAIMED)
        {
            return BadRequest("This listing is currently borrowed");
        }

        listing.Status = ListingStatus.CLAIMED;

        var payment = new Payment
        {
            Amount = request.Amount,
            Listing = listing,
            Recipient = listing.BNUser
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return Ok($"${request.Amount} was paid to {listing.BNUser.UserName}, initiating rental.");
    }

    [HttpPost("return")]
    public async Task<IActionResult> Return([FromBody] ReturnListingRequest request)
    {
        var listing = await _context.Listings
                                    .Include(l => l.BNUser)
                                    .FirstOrDefaultAsync(l => l.ID == request.ListingId);

        if (listing == null)
        {
            return NotFound("Listing not found.");
        }

        if (listing.Status == ListingStatus.AVAILABLE)
        {
            return BadRequest("This listing is already marked as available.");
        }

        listing.Status = ListingStatus.AVAILABLE;
        await _context.SaveChangesAsync();

        return Ok($"Listing {request.ListingId} has been returned and is now available.");
    }

}
