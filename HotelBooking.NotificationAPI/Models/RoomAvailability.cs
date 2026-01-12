namespace HotelBooking.NotificationAPI.Models;

public class RoomAvailability
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public int RoomId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int AvailableRooms { get; set; }
}
