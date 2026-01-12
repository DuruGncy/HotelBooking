using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.Models.DTOs;

public class CancelBookingRequest
{
    [Required]
    public int BookingId { get; set; }
    
    [StringLength(500)]
    public string? CancellationReason { get; set; }
}
