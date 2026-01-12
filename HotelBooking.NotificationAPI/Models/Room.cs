namespace HotelBooking.NotificationAPI.Models;

public class Room
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxOccupancy { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Amenities { get; set; } = new();
}
