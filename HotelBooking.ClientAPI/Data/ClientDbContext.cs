using Microsoft.EntityFrameworkCore;
using HotelBooking.ClientAPI.Data.Entities;

namespace HotelBooking.ClientAPI.Data;

public class ClientDbContext : DbContext
{
    public ClientDbContext(DbContextOptions<ClientDbContext> options) : base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomAvailability> RoomAvailabilities { get; set; }
    public DbSet<Booking> Bookings { get; set; }

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

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Hotel)
            .WithMany(h => h.Bookings)
            .HasForeignKey(b => b.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

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

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.BookingReference)
            .IsUnique();

        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.CheckInDate, b.CheckOutDate });

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.GuestEmail);
    }
}
