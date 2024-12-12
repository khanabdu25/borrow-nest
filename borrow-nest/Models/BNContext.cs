using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace borrow_nest.Models;

public class BNContext : IdentityDbContext
{
    public BNContext(DbContextOptions<BNContext> options)
        : base(options)
    {
    }

    public DbSet<BNUser> BNUsers { get; set; }

    public DbSet<Event> Events { get; set; }

    public DbSet<ClientCheckInLog> ClientCheckIns { get; set; }

}