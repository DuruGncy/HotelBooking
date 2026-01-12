using HotelBooking.ClientAPI.Models;

namespace HotelBooking.ClientAPI.Services.Interfaces;

/// <summary>
/// Interface for communicating with NotificationAPI
/// </summary>
public interface INotificationClient
{
    /// <summary>
    /// Send booking confirmation notification to NotificationAPI
    /// </summary>
    Task<bool> SendBookingConfirmationAsync(Booking booking);

    /// <summary>
    /// Send booking cancellation notification to NotificationAPI
    /// </summary>
    Task<bool> SendBookingCancellationAsync(Booking booking);
}
