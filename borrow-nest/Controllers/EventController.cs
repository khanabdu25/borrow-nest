using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using borrow_nest.Models;

namespace borrow_nest.Controllers;

[ApiController]
[Route("events")]
public class EventController : ControllerBase
{
    private readonly ILogger<EventController> _logger;

    private readonly BNContext _context;

    public EventController(ILogger<EventController> logger, BNContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var events = await _context.Events.ToListAsync();
        return Ok(events);
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        try
        {
            // Perform a simple database operation to test connectivity
            var isDatabaseConnected = _context.Database.CanConnect();

            if (isDatabaseConnected)
            {
                _logger.LogInformation("Database connection is successful.");
                return Ok("EventController is working and connected to the database.");
            }
            else
            {
                _logger.LogWarning("Database connection failed.");
                return StatusCode(500, "Unable to connect to the database.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while testing the controller.");
            return StatusCode(500, "An error occurred while testing the controller.");
        }
    }
}
