using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TraceMap.Data;

#nullable disable

namespace TraceMap.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
            {
                b.Property<string>("Id").HasColumnType("nvarchar(450)");
                b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
                b.Property<string>("Name").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<string>("NormalizedName").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.HasKey("Id");
                b.HasIndex("NormalizedName").IsUnique().HasDatabaseName("RoleNameIndex").HasFilter("[NormalizedName] IS NOT NULL");
                b.ToTable("AspNetRoles", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
                b.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
                b.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
                b.Property<string>("RoleId").IsRequired().HasColumnType("nvarchar(450)");
                b.HasKey("Id");
                b.HasIndex("RoleId");
                b.ToTable("AspNetRoleClaims", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
                b.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
                b.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
                b.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
                b.HasKey("Id");
                b.HasIndex("UserId");
                b.ToTable("AspNetUserClaims", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
            {
                b.Property<string>("LoginProvider").HasColumnType("nvarchar(450)");
                b.Property<string>("ProviderKey").HasColumnType("nvarchar(450)");
                b.Property<string>("ProviderDisplayName").HasColumnType("nvarchar(max)");
                b.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
                b.HasKey("LoginProvider", "ProviderKey");
                b.HasIndex("UserId");
                b.ToTable("AspNetUserLogins", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
            {
                b.Property<string>("UserId").HasColumnType("nvarchar(450)");
                b.Property<string>("RoleId").HasColumnType("nvarchar(450)");
                b.HasKey("UserId", "RoleId");
                b.HasIndex("RoleId");
                b.ToTable("AspNetUserRoles", (string)null);
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
            {
                b.Property<string>("UserId").HasColumnType("nvarchar(450)");
                b.Property<string>("LoginProvider").HasColumnType("nvarchar(450)");
                b.Property<string>("Name").HasColumnType("nvarchar(450)");
                b.Property<string>("Value").HasColumnType("nvarchar(max)");
                b.HasKey("UserId", "LoginProvider", "Name");
                b.ToTable("AspNetUserTokens", (string)null);
            });

            modelBuilder.Entity("TraceMap.Models.ApplicationUser", b =>
            {
                b.Property<string>("Id").HasColumnType("nvarchar(450)");
                b.Property<int>("AccessFailedCount").HasColumnType("int");
                b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
                b.Property<string>("DisplayName").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<string>("Email").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<bool>("EmailConfirmed").HasColumnType("bit");
                b.Property<bool>("LockoutEnabled").HasColumnType("bit");
                b.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("datetimeoffset");
                b.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<string>("PasswordHash").HasColumnType("nvarchar(max)");
                b.Property<string>("PhoneNumber").HasColumnType("nvarchar(max)");
                b.Property<bool>("PhoneNumberConfirmed").HasColumnType("bit");
                b.Property<string>("SecurityStamp").HasColumnType("nvarchar(max)");
                b.Property<bool>("TwoFactorEnabled").HasColumnType("bit");
                b.Property<string>("UserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.HasKey("Id");
                b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
                b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex").HasFilter("[NormalizedUserName] IS NOT NULL");
                b.ToTable("AspNetUsers", (string)null);
            });

            modelBuilder.Entity("TraceMap.Models.PlaceComment", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
                b.Property<string>("Content").IsRequired().HasMaxLength(1000).HasColumnType("nvarchar(1000)");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<bool>("IsAnonymous").HasColumnType("bit");
                b.Property<int>("TracePlaceId").HasColumnType("int");
                b.Property<string>("UserId").HasColumnType("nvarchar(max)");
                b.Property<string>("UserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.HasKey("Id");
                b.HasIndex("TracePlaceId");
                b.ToTable("PlaceComments");
            });

            modelBuilder.Entity("TraceMap.Models.PlaceLike", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<int>("TracePlaceId").HasColumnType("int");
                b.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
                b.Property<string>("UserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.HasKey("Id");
                b.HasIndex("TracePlaceId", "UserId").IsUnique();
                b.ToTable("PlaceLikes");
            });

            modelBuilder.Entity("TraceMap.Models.PlacePhoto", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
                b.Property<string>("BlobETag").HasMaxLength(500).HasColumnType("nvarchar(500)");
                b.Property<string>("BlobName").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
                b.Property<string>("ContentType").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<string>("FileName").IsRequired().HasMaxLength(260).HasColumnType("nvarchar(260)");
                b.Property<bool>("IsAnonymous").HasColumnType("bit");
                b.Property<bool>("IsDeleted").HasColumnType("bit");
                b.Property<string>("LocalRelativePath").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
                b.Property<long>("Size").HasColumnType("bigint");
                b.Property<string>("StorageProvider").IsRequired().HasMaxLength(32).HasColumnType("nvarchar(32)");
                b.Property<int>("TracePlaceId").HasColumnType("int");
                b.Property<DateTime>("UpdatedAt").HasColumnType("datetime2");
                b.Property<string>("UserId").HasColumnType("nvarchar(max)");
                b.Property<string>("UserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.HasKey("Id");
                b.HasIndex("TracePlaceId");
                b.ToTable("PlacePhotos");
            });

            modelBuilder.Entity("TraceMap.Models.PlaceReview", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
                b.Property<string>("Content").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<string>("PhotoUrl").HasColumnType("nvarchar(max)");
                b.Property<int>("TracePlaceId").HasColumnType("int");
                b.HasKey("Id");
                b.HasIndex("TracePlaceId");
                b.ToTable("PlaceReviews");
            });

            modelBuilder.Entity("TraceMap.Models.TracePlace", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));
                b.Property<string>("Category").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
                b.Property<string>("Description").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<bool>("IsAnonymous").HasColumnType("bit");
                b.Property<bool>("IsShared").HasColumnType("bit");
                b.Property<bool>("IsVisited").HasColumnType("bit");
                b.Property<double>("Latitude").HasColumnType("float");
                b.Property<double>("Longitude").HasColumnType("float");
                b.Property<string>("Name").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<string>("PhotoUrl").HasColumnType("nvarchar(max)");
                b.Property<string>("RecommendedActivities").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<string>("SharedDescription").IsRequired().HasColumnType("nvarchar(max)");
                b.Property<DateTime>("UpdatedAt").HasColumnType("datetime2");
                b.Property<string>("UserId").HasColumnType("nvarchar(max)");
                b.Property<string>("UserName").HasColumnType("nvarchar(max)");
                b.Property<int>("VisitCount").HasColumnType("int");
                b.HasKey("Id");
                b.HasIndex("CreatedAt");
                b.HasIndex("IsShared");
                b.ToTable("Places");
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
            {
                b.HasOne("TraceMap.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
            {
                b.HasOne("TraceMap.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
            {
                b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                    .WithMany()
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.HasOne("TraceMap.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
            {
                b.HasOne("TraceMap.Models.ApplicationUser", null)
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("TraceMap.Models.PlaceComment", b =>
            {
                b.HasOne("TraceMap.Models.TracePlace", "TracePlace")
                    .WithMany()
                    .HasForeignKey("TracePlaceId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("TracePlace");
            });

            modelBuilder.Entity("TraceMap.Models.PlaceLike", b =>
            {
                b.HasOne("TraceMap.Models.TracePlace", "TracePlace")
                    .WithMany()
                    .HasForeignKey("TracePlaceId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("TracePlace");
            });

            modelBuilder.Entity("TraceMap.Models.PlacePhoto", b =>
            {
                b.HasOne("TraceMap.Models.TracePlace", "TracePlace")
                    .WithMany()
                    .HasForeignKey("TracePlaceId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("TracePlace");
            });

            modelBuilder.Entity("TraceMap.Models.PlaceReview", b =>
            {
                b.HasOne("TraceMap.Models.TracePlace", "TracePlace")
                    .WithMany()
                    .HasForeignKey("TracePlaceId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.Navigation("TracePlace");
            });
#pragma warning restore 612, 618
        }
    }
}
