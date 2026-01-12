using HotelBookingAPI.Models.DTOs;

namespace HotelBookingAPI.Services.Interfaces;

public interface IBookHotelService
{
    Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request, int? userId = null);
    Task<BookingResponse?> GetBookingByIdAsync(int bookingId);
    Task<BookingResponse?> GetBookingByReferenceAsync(string bookingReference);
    Task<IEnumerable<BookingResponse>> GetUserBookingsAsync(int userId);
    Task<BookingResponse> CancelBookingAsync(int bookingId, string? cancellationReason = null);
    Task<IEnumerable<BookingResponse>> GetHotelBookingsAsync(int hotelId, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> CheckAvailabilityAsync(int hotelId, int roomId, DateTime checkIn, DateTime checkOut, int numberOfRooms);
}
