using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Models.DTOs;

public class AddRoomAvailabilityRequest
{
    [Required]
    public int HotelId { get; set; }
    
    [Required]
    public int RoomId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Available rooms must be at least 1")]
    public int AvailableRooms { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Price per night must be non-negative")]
    public decimal PricePerNight { get; set; }
}
