using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.AdminAPI.Data.Entities;

[Table("room_availabilities")]
public class RoomAvailability
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("hotel_id")]
    public int HotelId { get; set; }

    [Required]
    [Column("room_id")]
    public int RoomId { get; set; }

    [Required]
    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Required]
    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Required]
    [Column("available_rooms")]
    public int AvailableRooms { get; set; }

    [Required]
    [Column("price_per_night", TypeName = "decimal(10, 2)")]
    public decimal PricePerNight { get; set; }

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
