namespace HotelBookingAPI.Models.DTOs;

public class RoomAvailabilityResponse
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string? HotelName { get; set; }
    public int RoomId { get; set; }
    public string? RoomType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int AvailableRooms { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
