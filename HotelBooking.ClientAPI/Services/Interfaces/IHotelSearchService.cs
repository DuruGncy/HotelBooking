using HotelBooking.ClientAPI.Models.DTOs;

namespace HotelBooking.ClientAPI.Services.Interfaces;

public interface IHotelSearchService
{
    Task<IEnumerable<HotelSearchResponse>> SearchHotelsAsync(HotelSearchRequest request);
    Task<HotelSearchResponse?> GetHotelDetailsAsync(int hotelId, DateTime checkInDate, DateTime checkOutDate, int numberOfGuests);
}
