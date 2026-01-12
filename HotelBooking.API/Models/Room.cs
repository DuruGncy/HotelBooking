namespace HotelBookingAPI.Models;

public class Room
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public int MaxOccupancy { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Amenities { get; set; } = new();
    public Hotel? Hotel { get; set; }
    public List<RoomAvailability> Availabilities { get; set; } = new();
}
