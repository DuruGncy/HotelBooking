using System.ComponentModel.DataAnnotations;

namespace HotelBooking.NotificationAPI.Models.DTOs;

public class BookingNotificationRequest
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    public int HotelId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string GuestEmail { get; set; } = string.Empty;

    [Required]
    public string BookingReference { get; set; } = string.Empty;

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    public int NumberOfRooms { get; set; }

    [Required]
    public int NumberOfGuests { get; set; }

    [Required]
    public decimal TotalPrice { get; set; }

    public string? SpecialRequests { get; set; }
}

public class BookingCancellationRequest
{
    [Required]
    public int BookingId { get; set; }

    [Required]
    public int HotelId { get; set; }

    [Required]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string GuestEmail { get; set; } = string.Empty;

    [Required]
    public string BookingReference { get; set; } = string.Empty;

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    public DateTime? CancellationDate { get; set; }

    public string? CancellationReason { get; set; }
}
