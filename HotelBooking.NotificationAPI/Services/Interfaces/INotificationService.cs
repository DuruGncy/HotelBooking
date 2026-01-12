using HotelBooking.NotificationAPI.Models;
using HotelBooking.NotificationAPI.Models.DTOs;

namespace HotelBooking.NotificationAPI.Services.Interfaces;

public interface INotificationService
{
    // Notification creation
    Task<Notification> CreateBookingConfirmationNotificationAsync(Booking booking);
    Task<Notification> CreateBookingCancellationNotificationAsync(Booking booking);
    Task<Notification> CreateLowCapacityAlertAsync(LowCapacityAlert alert);
    
    // Notification processing
    Task ProcessNotificationQueueAsync();
    Task<bool> SendNotificationAsync(Notification notification);
    
    // Scheduled tasks
    Task CheckLowCapacityAndNotifyAsync();
    Task ProcessNewReservationsAsync();
    
    // Notification retrieval
    Task<IEnumerable<NotificationResponse>> GetHotelNotificationsAsync(int hotelId);
    Task<IEnumerable<NotificationResponse>> GetPendingNotificationsAsync();
    Task<NotificationResponse?> GetNotificationByIdAsync(int notificationId);
}
