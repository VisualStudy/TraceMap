using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TraceMap.Models;

namespace TraceMap.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TracePlace> Places => Set<TracePlace>();
    public DbSet<PlaceReview> PlaceReviews => Set<PlaceReview>();
}
