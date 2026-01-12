namespace HotelBooking.NotificationAPI.Models;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int HotelId { get; set; }
    public int RoomId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfRooms { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime BookingDate { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string? GuestPhone { get; set; }
    public string? SpecialRequests { get; set; }
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    CheckedIn,
    CheckedOut,
    Cancelled
}
