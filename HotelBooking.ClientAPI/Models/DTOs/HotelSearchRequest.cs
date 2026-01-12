using System.ComponentModel.DataAnnotations;

namespace HotelBooking.ClientAPI.Models.DTOs;

public class HotelSearchRequest
{
    [Required(ErrorMessage = "Destination is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Destination must be between 2 and 100 characters")]
    public string Destination { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Check-in date is required")]
    public DateTime CheckInDate { get; set; }
    
    [Required(ErrorMessage = "Check-out date is required")]
    public DateTime CheckOutDate { get; set; }
    
    [Required(ErrorMessage = "Number of guests is required")]
    [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
    public int NumberOfGuests { get; set; }
    
    [Range(1, 10, ErrorMessage = "Number of rooms must be between 1 and 10")]
    public int NumberOfRooms { get; set; } = 1;
}
