using Microsoft.EntityFrameworkCore;
using HotelBooking.NotificationAPI.Data.Entities;

namespace HotelBooking.NotificationAPI.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Hotel)
            .WithMany(h => h.Bookings)
            .HasForeignKey(b => b.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.RelatedBooking)
            .WithMany(b => b.Notifications)
            .HasForeignKey(n => n.RelatedBookingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.RelatedHotel)
            .WithMany(h => h.Notifications)
            .HasForeignKey(n => n.RelatedHotelId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure indexes
        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.BookingReference)
            .IsUnique();

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.GuestEmail);

        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.Status);

        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.RecipientEmail);

        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.NotificationType);

        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.ScheduledFor);
    }
}
