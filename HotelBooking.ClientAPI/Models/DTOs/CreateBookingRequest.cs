using System.ComponentModel.DataAnnotations;

namespace HotelBooking.ClientAPI.Models.DTOs;

public class CreateBookingRequest
{
    [Required]
    public int HotelId { get; set; }
    
    [Required]
    public int RoomId { get; set; }
    
    [Required]
    public DateTime CheckInDate { get; set; }
    
    [Required]
    public DateTime CheckOutDate { get; set; }
    
    [Required]
    [Range(1, 10, ErrorMessage = "Number of rooms must be between 1 and 10")]
    public int NumberOfRooms { get; set; }
    
    [Required]
    [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
    public int NumberOfGuests { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string GuestName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string GuestEmail { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string GuestPhone { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? SpecialRequests { get; set; }
}
