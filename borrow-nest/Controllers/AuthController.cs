using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using borrow_nest.Models;
using borrow_nest.Services;

namespace borrow_nest.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(UserManager<BNUser> userManager, SignInManager<BNUser> signInManager, IConfiguration configuration, RoleCheckerService roleChecker) : ControllerBase
{
    private readonly UserManager<BNUser> _userManager = userManager;
    private readonly SignInManager<BNUser> _signInManager = signInManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly RoleCheckerService _roleChecker = roleChecker;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] Registration payload)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (payload.Password != payload.ConfirmPassword)
        {
            return BadRequest(new { error = "Passwords do not match." });
        }

        var user = new BNUser { UserName = payload.Email, Email = payload.Email };

        var result = await _userManager.CreateAsync(user, payload.Password);

        if (result.Succeeded)
        {
            return Ok(new { message = "Registration successful." });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] Login creds)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _signInManager.PasswordSignInAsync(creds.Email, creds.Password, creds.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(creds.Email);
            if (user == null)
            {
                return Unauthorized(new { error = "User not found." });
            }

            if (!await _roleChecker.HasRoleAsync("User"))
            {
                // by default they all have the "User" role
                // their higher ups will add more roles as necessary
                await _userManager.AddToRoleAsync(user, "User");
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful.",
                token,
                userId = user.Id,
                email = user.Email
            });
        }

        return Unauthorized(new { error = "Invalid login attempt." });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logout successful." });
    }

    private async Task<string> GenerateJwtToken(BNUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.ToUpper())));

        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("JWT key is not configured properly.");
        }

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}