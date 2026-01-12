namespace HotelBooking.NotificationAPI.Models;

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public List<string> Amenities { get; set; } = new();
    public string ImageUrl { get; set; } = string.Empty;
}
