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
        var events = await _context.Listings.ToListAsync();
        return Ok(events);
    }

    [HttpPost("create")]
    [Authorize(Roles = "USER")]
    public async Task<IActionResult> CreateListing([FromBody] CreateListingRequest request)
    {
        try
        {
            // Get the current user using the provided _roleChecker method
            BNUser user = await _roleChecker.GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized("User must be logged in to create a listing.");
            }

            // Create and populate the product from the request
            Product product = new Product
            {
                Status = request.Product.Status,
                HourlyRate = request.Product.HourlyRate,
                DailyRate = request.Product.DailyRate,
                WeeklyRate = request.Product.WeeklyRate,
                MonthlyRate = request.Product.MonthlyRate
            };

            // Create and populate the listing with the associated product
            Listing listing = new Listing
            {
                Title = request.Title,
                Body = request.Body,
                BNUser = user,  // Associate the current user with the listing
                Product = product  // Associate the product with the listing
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



}
