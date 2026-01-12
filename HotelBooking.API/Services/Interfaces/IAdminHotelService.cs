using HotelBookingAPI.Models.DTOs;

namespace HotelBookingAPI.Services.Interfaces;

public interface IAdminHotelService
{
    Task<RoomAvailabilityResponse> AddRoomAvailabilityAsync(AddRoomAvailabilityRequest request);
    Task<RoomAvailabilityResponse> UpdateRoomAvailabilityAsync(UpdateRoomAvailabilityRequest request);
    Task<bool> DeleteRoomAvailabilityAsync(int availabilityId);
    Task<RoomAvailabilityResponse?> GetRoomAvailabilityByIdAsync(int availabilityId);
    Task<IEnumerable<RoomAvailabilityResponse>> GetRoomAvailabilitiesByHotelAsync(int hotelId);
}
