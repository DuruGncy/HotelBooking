using Microsoft.EntityFrameworkCore;
using HotelBooking.AdminAPI.Data.Entities;

namespace HotelBooking.AdminAPI.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomAvailability> RoomAvailabilities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PostgreSQL array columns
        modelBuilder.Entity<Hotel>()
            .Property(h => h.Amenities)
            .HasColumnType("text[]");

        modelBuilder.Entity<Room>()
            .Property(r => r.Amenities)
            .HasColumnType("text[]");

        // Configure relationships
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoomAvailability>()
            .HasOne(ra => ra.Hotel)
            .WithMany(h => h.RoomAvailabilities)
            .HasForeignKey(ra => ra.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoomAvailability>()
            .HasOne(ra => ra.Room)
            .WithMany(r => r.RoomAvailabilities)
            .HasForeignKey(ra => ra.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes
        modelBuilder.Entity<Hotel>()
            .HasIndex(h => h.Location);

        modelBuilder.Entity<Hotel>()
            .HasIndex(h => h.StarRating);

        modelBuilder.Entity<Room>()
            .HasIndex(r => r.HotelId);

        modelBuilder.Entity<RoomAvailability>()
            .HasIndex(ra => new { ra.StartDate, ra.EndDate });

        modelBuilder.Entity<RoomAvailability>()
            .HasIndex(ra => new { ra.HotelId, ra.RoomId });

        modelBuilder.Entity<RoomAvailability>()
            .HasIndex(ra => new { ra.HotelId, ra.RoomId, ra.StartDate, ra.EndDate })
            .IsUnique();
    }
}
