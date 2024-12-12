using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using borrow_nest.Models;
using Microsoft.AspNetCore.Identity;
using borrow_nest.Services;

namespace borrow_nest.Controllers;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    private readonly BNContext _context;
    private readonly UserManager<BNUser> _userManager;

    private readonly RoleCheckerService _roleChecker;

    public UserController(ILogger<UserController> logger, BNContext context, UserManager<BNUser> userManager, RoleCheckerService roleChecker)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleChecker = roleChecker;
    }

    [HttpGet("all")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.BNUsers.ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "USER,ADMIN")]
    public async Task<ActionResult<BNUser>> GetUser(string id)
    {
        // only admins can see all users
        // otherwise they can only see themselves
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        if (!await _roleChecker.HasRoleAsync("Admin") && user != await _roleChecker.GetCurrentUserAsync())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "You do not have the permission to see this user."
            });
        }

        return user;
    }
}
