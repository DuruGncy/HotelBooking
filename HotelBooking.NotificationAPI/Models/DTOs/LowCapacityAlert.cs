namespace HotelBooking.NotificationAPI.Models.DTOs;

public class LowCapacityAlert
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int TotalCapacity { get; set; }
    public int AvailableRooms { get; set; }
    public decimal CapacityPercentage { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
}
