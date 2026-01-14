using HotelBooking.AdminAPI.Models.DTOs;
using HotelBooking.AdminAPI.Data.Entities;

namespace HotelBooking.AdminAPI.Services.Interfaces;

public interface IAdminHotelService
{
    Task<RoomAvailabilityResponse> AddRoomAvailabilityAsync(AddRoomAvailabilityRequest request);
    Task<RoomAvailabilityResponse> UpdateRoomAvailabilityAsync(UpdateRoomAvailabilityRequest request);
    Task<bool> DeleteRoomAvailabilityAsync(int availabilityId);
    Task<RoomAvailabilityResponse?> GetRoomAvailabilityByIdAsync(int availabilityId);
    Task<IEnumerable<RoomAvailabilityResponse>> GetRoomAvailabilitiesByHotelAsync(int hotelId);
    
    // New methods for getting data
    Task<IEnumerable<Hotel>> GetAllHotelsAsync();
    Task<IEnumerable<Room>> GetAllRoomsAsync();
    Task<IEnumerable<RoomAvailability>> GetAllAvailabilitiesAsync();
}
