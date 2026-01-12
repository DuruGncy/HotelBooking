using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Models.DTOs;

public class UpdateRoomAvailabilityRequest
{
    [Required]
    public int AvailabilityId { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Available rooms cannot be negative")]
    public int? AvailableRooms { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Price per night must be non-negative")]
    public decimal? PricePerNight { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}
