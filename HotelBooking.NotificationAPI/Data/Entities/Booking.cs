using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.NotificationAPI.Data.Entities;

[Table("bookings")]
public class Booking
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("booking_reference")]
    public string BookingReference { get; set; } = string.Empty;

    [Required]
    [Column("hotel_id")]
    public int HotelId { get; set; }

    [Required]
    [Column("room_id")]
    public int RoomId { get; set; }

    [Required]
    [Column("check_in_date")]
    public DateTime CheckInDate { get; set; }

    [Required]
    [Column("check_out_date")]
    public DateTime CheckOutDate { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("guest_name")]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("guest_email")]
    public string GuestEmail { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = "Pending";

    [Column("booking_date")]
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("HotelId")]
    public virtual Hotel Hotel { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
