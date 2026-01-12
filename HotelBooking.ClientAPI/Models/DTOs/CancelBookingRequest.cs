using System.ComponentModel.DataAnnotations;

namespace HotelBooking.ClientAPI.Models.DTOs;

public class CancelBookingRequest
{
    [StringLength(500)]
    public string? CancellationReason { get; set; }
}
