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
    public DbSet<PlaceComment> PlaceComments => Set<PlaceComment>();
    public DbSet<PlaceLike> PlaceLikes => Set<PlaceLike>();
    public DbSet<PlacePhoto> PlacePhotos => Set<PlacePhoto>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PlaceLike>()
            .HasIndex(like => new { like.TracePlaceId, like.UserId })
            .IsUnique();

        builder.Entity<PlacePhoto>()
            .HasIndex(photo => photo.TracePlaceId);

        builder.Entity<PlacePhoto>()
            .HasOne(photo => photo.TracePlace)
            .WithMany()
            .HasForeignKey(photo => photo.TracePlaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
