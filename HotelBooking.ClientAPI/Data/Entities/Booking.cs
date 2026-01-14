using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.ClientAPI.Data.Entities;

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

    [Column("user_id")]
    public int? UserId { get; set; }

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
    [Column("number_of_rooms")]
    public int NumberOfRooms { get; set; }

    [Required]
    [Column("number_of_guests")]
    public int NumberOfGuests { get; set; }

    [Required]
    [Column("total_price", TypeName = "decimal(10, 2)")]
    public decimal TotalPrice { get; set; }

    [Required]
    [Column("price_per_night", TypeName = "decimal(10, 2)")]
    public decimal PricePerNight { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, CheckedIn, CheckedOut, Cancelled, NoShow

    [Column("booking_date")]
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    [Column("cancellation_date")]
    public DateTime? CancellationDate { get; set; }

    [Column("cancellation_reason")]
    public string? CancellationReason { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("guest_name")]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("guest_email")]
    public string GuestEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("guest_phone")]
    public string GuestPhone { get; set; } = string.Empty;

    [Column("special_requests")]
    public string? SpecialRequests { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("HotelId")]
    public virtual Hotel Hotel { get; set; } = null!;

    [ForeignKey("RoomId")]
    public virtual Room Room { get; set; } = null!;
}
