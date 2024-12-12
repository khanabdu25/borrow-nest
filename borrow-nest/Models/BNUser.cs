using Microsoft.AspNetCore.Identity;

namespace borrow_nest.Models;

public class BNUser : IdentityUser
{
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
}