using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.ClientAPI.Data.Entities;

[Table("rooms")]
public class Room
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("hotel_id")]
    public int HotelId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("room_type")]
    public string RoomType { get; set; } = string.Empty;

    [Required]
    [Column("max_occupancy")]
    public int MaxOccupancy { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("amenities")]
    public List<string> Amenities { get; set; } = new();

    [Column("size_sqm")]
    public decimal? SizeSqm { get; set; }

    [MaxLength(50)]
    [Column("bed_type")]
    public string? BedType { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("HotelId")]
    public virtual Hotel Hotel { get; set; } = null!;
    
    public virtual ICollection<RoomAvailability> RoomAvailabilities { get; set; } = new List<RoomAvailability>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
