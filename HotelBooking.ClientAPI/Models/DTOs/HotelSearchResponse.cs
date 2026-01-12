namespace HotelBooking.ClientAPI.Models.DTOs;

public class HotelSearchResponse
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public List<AvailableRoomInfo> AvailableRooms { get; set; } = new();
    public decimal LowestPricePerNight { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalAvailableRooms { get; set; }
}

public class AvailableRoomInfo
{
    public int RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxOccupancy { get; set; }
    public List<string> Amenities { get; set; } = new();
    public decimal PricePerNight { get; set; }
    public int AvailableCount { get; set; }
    public decimal TotalPrice { get; set; }
}
