using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.NotificationAPI.Data.Entities;

[Table("notifications")]
public class Notification
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("notification_type")]
    public string NotificationType { get; set; } = string.Empty; // BookingConfirmation, BookingCancellation, LowCapacityAlert, etc.

    [Required]
    [MaxLength(255)]
    [Column("recipient_email")]
    public string RecipientEmail { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column("recipient_name")]
    public string? RecipientName { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("subject")]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("related_booking_id")]
    public int? RelatedBookingId { get; set; }

    [Column("related_hotel_id")]
    public int? RelatedHotelId { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Pending"; // Pending, Sent, Failed, Retrying

    [Column("retry_count")]
    public int RetryCount { get; set; } = 0;

    [Column("scheduled_for")]
    public DateTime? ScheduledFor { get; set; }

    [Column("sent_at")]
    public DateTime? SentAt { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("RelatedBookingId")]
    public virtual Booking? RelatedBooking { get; set; }

    [ForeignKey("RelatedHotelId")]
    public virtual Hotel? RelatedHotel { get; set; }
}
