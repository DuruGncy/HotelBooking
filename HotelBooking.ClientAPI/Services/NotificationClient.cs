using HotelBooking.ClientAPI.Models;
using HotelBooking.ClientAPI.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace HotelBooking.ClientAPI.Services;

public class NotificationClient : INotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationClient> _logger;

    public NotificationClient(HttpClient httpClient, ILogger<NotificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendBookingConfirmationAsync(Booking booking)
    {
        try
        {
            _logger.LogInformation("Sending booking confirmation notification for booking {BookingId}", booking.Id);

            var payload = new
            {
                bookingId = booking.Id,
                hotelId = booking.HotelId,
                roomId = booking.RoomId,
                guestName = booking.GuestName,
                guestEmail = booking.GuestEmail,
                bookingReference = booking.BookingReference,
                checkInDate = booking.CheckInDate,
                checkOutDate = booking.CheckOutDate,
                numberOfRooms = booking.NumberOfRooms,
                numberOfGuests = booking.NumberOfGuests,
                totalPrice = booking.TotalPrice,
                specialRequests = booking.SpecialRequests
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1.0/Notifications/booking-confirmation", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully sent booking confirmation notification for booking {BookingId}", booking.Id);
                return true;
            }

            _logger.LogWarning("Failed to send booking confirmation notification for booking {BookingId}. Status: {StatusCode}",
                booking.Id, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending booking confirmation notification for booking {BookingId}", booking.Id);
            return false;
        }
    }

    public async Task<bool> SendBookingCancellationAsync(Booking booking)
    {
        try
        {
            _logger.LogInformation("Sending booking cancellation notification for booking {BookingId}", booking.Id);

            var payload = new
            {
                bookingId = booking.Id,
                hotelId = booking.HotelId,
                guestName = booking.GuestName,
                guestEmail = booking.GuestEmail,
                bookingReference = booking.BookingReference,
                checkInDate = booking.CheckInDate,
                checkOutDate = booking.CheckOutDate,
                cancellationDate = booking.CancellationDate,
                cancellationReason = booking.CancellationReason
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1.0/Notifications/booking-cancellation", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully sent booking cancellation notification for booking {BookingId}", booking.Id);
                return true;
            }

            _logger.LogWarning("Failed to send booking cancellation notification for booking {BookingId}. Status: {StatusCode}",
                booking.Id, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending booking cancellation notification for booking {BookingId}", booking.Id);
            return false;
        }
    }
}
