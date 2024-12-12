using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using borrow_nest.Models;

namespace borrow_nest.Services;

public class RoleCheckerService(UserManager<BNUser> userManager, IHttpContextAccessor httpContextAccessor, ILogger<RoleCheckerService> logger)
{
    private readonly UserManager<BNUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<RoleCheckerService> _logger = logger;

    public async Task<bool> HasRoleAsync(string roleName)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return false;
        }

        return await _userManager.IsInRoleAsync(user, roleName);
    }

    public async Task<BNUser?> GetCurrentUserAsync()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && Guid.TryParse(c.Value, out _));

        var userId = userIdClaim?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await _userManager.FindByIdAsync(userId);
    }

    public async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "User", "Manager" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
