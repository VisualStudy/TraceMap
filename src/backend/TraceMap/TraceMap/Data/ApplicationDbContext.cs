using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TraceMap.Models;

namespace TraceMap.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IConfiguration? _configuration;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration? configuration = null) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<TracePlace> Places => Set<TracePlace>();
    public DbSet<PlaceReview> PlaceReviews => Set<PlaceReview>();
    public DbSet<PlaceComment> PlaceComments => Set<PlaceComment>();
    public DbSet<PlaceLike> PlaceLikes => Set<PlaceLike>();
    public DbSet<PlacePhoto> PlacePhotos => Set<PlacePhoto>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var connectionString = _configuration?.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TracePlace>()
            .HasIndex(place => place.IsShared);

        builder.Entity<TracePlace>()
            .HasIndex(place => place.CreatedAt);

        builder.Entity<PlaceComment>()
            .HasIndex(comment => comment.TracePlaceId);

        builder.Entity<PlaceComment>()
            .HasOne(comment => comment.TracePlace)
            .WithMany()
            .HasForeignKey(comment => comment.TracePlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PlaceLike>()
            .HasIndex(like => new { like.TracePlaceId, like.UserId })
            .IsUnique();

        builder.Entity<PlaceLike>()
            .HasOne(like => like.TracePlace)
            .WithMany()
            .HasForeignKey(like => like.TracePlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PlacePhoto>()
            .HasIndex(photo => photo.TracePlaceId);

        builder.Entity<PlacePhoto>()
            .HasOne(photo => photo.TracePlace)
            .WithMany()
            .HasForeignKey(photo => photo.TracePlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PlaceReview>()
            .HasIndex(review => review.TracePlaceId);

        builder.Entity<PlaceReview>()
            .HasOne(review => review.TracePlace)
            .WithMany()
            .HasForeignKey(review => review.TracePlaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
