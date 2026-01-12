namespace HotelBooking.AdminAPI.Models;

public class RoomAvailability
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int HotelId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int AvailableRooms { get; set; }
    public decimal PricePerNight { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Room? Room { get; set; }
    public Hotel? Hotel { get; set; }
}
