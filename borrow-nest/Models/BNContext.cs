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

    public DbSet<Listing> Listings { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<CarListing> CarListings { get; set; }

}