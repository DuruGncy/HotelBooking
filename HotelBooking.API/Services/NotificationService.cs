using HotelBookingAPI.Models;
using HotelBookingAPI.Models.DTOs;
using HotelBookingAPI.Services.Interfaces;
using System.Collections.Concurrent;

namespace HotelBookingAPI.Services;

public class NotificationService : INotificationService
{
    // In-memory storage for demo (replace with database in production)
    private static readonly List<Notification> _notifications = new();
    private static readonly ConcurrentQueue<Notification> _notificationQueue = new();
    private static int _nextNotificationId = 1;
    
    // Hotel admin emails (in production, load from database)
    private static readonly Dictionary<int, string> _hotelAdminEmails = new()
    {
        { 1, "admin@grandplaza.com" },
        { 2, "admin@seasideresort.com" }
    };

    // Access to other services' data
    private static List<Hotel> _hotels = new();
    private static List<Room> _rooms = new();
    private static List<RoomAvailability> _availabilities = new();
    private static List<Booking> _bookings = new();

    public static void InitializeData(
        List<Hotel> hotels, 
        List<Room> rooms, 
        List<RoomAvailability> availabilities,
        List<Booking> bookings)
    {
        _hotels = hotels;
        _rooms = rooms;
        _availabilities = availabilities;
        _bookings = bookings;
    }

    #region Notification Creation

    public async Task<Notification> CreateBookingConfirmationNotificationAsync(Booking booking)
    {
        var hotel = _hotels.FirstOrDefault(h => h.Id == booking.HotelId);
        var room = _rooms.FirstOrDefault(r => r.Id == booking.RoomId);

        var notification = new Notification
        {
            Id = _nextNotificationId++,
            Type = NotificationType.BookingConfirmation,
            UserId = booking.UserId,
            HotelId = booking.HotelId,
            BookingId = booking.Id,
            Title = "Booking Confirmation",
            Message = GenerateBookingConfirmationMessage(booking, hotel, room),
            RecipientEmail = booking.GuestEmail,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                { "BookingReference", booking.BookingReference },
                { "HotelName", hotel?.Name ?? "Unknown" },
                { "CheckInDate", booking.CheckInDate.ToString("yyyy-MM-dd") },
                { "CheckOutDate", booking.CheckOutDate.ToString("yyyy-MM-dd") }
            }
        };

        _notifications.Add(notification);
        _notificationQueue.Enqueue(notification);

        return await Task.FromResult(notification);
    }

    public async Task<Notification> CreateBookingCancellationNotificationAsync(Booking booking)
    {
        var hotel = _hotels.FirstOrDefault(h => h.Id == booking.HotelId);

        var notification = new Notification
        {
            Id = _nextNotificationId++,
            Type = NotificationType.BookingCancellation,
            UserId = booking.UserId,
            HotelId = booking.HotelId,
            BookingId = booking.Id,
            Title = "Booking Cancellation Confirmation",
            Message = GenerateBookingCancellationMessage(booking, hotel),
            RecipientEmail = booking.GuestEmail,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                { "BookingReference", booking.BookingReference },
                { "HotelName", hotel?.Name ?? "Unknown" }
            }
        };

        _notifications.Add(notification);
        _notificationQueue.Enqueue(notification);

        return await Task.FromResult(notification);
    }

    public async Task<Notification> CreateLowCapacityAlertAsync(LowCapacityAlert alert)
    {
        var notification = new Notification
        {
            Id = _nextNotificationId++,
            Type = NotificationType.LowCapacityAlert,
            HotelId = alert.HotelId,
            Title = $"Low Capacity Alert - {alert.HotelName}",
            Message = GenerateLowCapacityMessage(alert),
            RecipientEmail = alert.AdminEmail,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, string>
            {
                { "HotelId", alert.HotelId.ToString() },
                { "RoomId", alert.RoomId.ToString() },
                { "Date", alert.Date.ToString("yyyy-MM-dd") },
                { "CapacityPercentage", alert.CapacityPercentage.ToString("F2") }
            }
        };

        _notifications.Add(notification);
        _notificationQueue.Enqueue(notification);

        return await Task.FromResult(notification);
    }

    #endregion

    #region Scheduled Tasks

    public async Task CheckLowCapacityAndNotifyAsync()
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Running low capacity check...");

        var startDate = DateTime.UtcNow.Date.AddDays(1); // Tomorrow
        var endDate = startDate.AddMonths(1); // Next month
        var lowCapacityThreshold = 0.20m; // 20%

        var alerts = new List<LowCapacityAlert>();

        // Check each hotel's capacity
        foreach (var hotel in _hotels)
        {
            var hotelRooms = _rooms.Where(r => r.HotelId == hotel.Id).ToList();
            
            foreach (var room in hotelRooms)
            {
                // Get availabilities for next month
                var roomAvailabilities = _availabilities
                    .Where(a => 
                        a.HotelId == hotel.Id && 
                        a.RoomId == room.Id &&
                        a.StartDate <= endDate &&
                        a.EndDate >= startDate)
                    .ToList();

                foreach (var availability in roomAvailabilities)
                {
                    // Check dates within our range
                    var checkStartDate = availability.StartDate < startDate ? startDate : availability.StartDate;
                    var checkEndDate = availability.EndDate > endDate ? endDate : availability.EndDate;

                    // Determine original capacity (in production, store this separately)
                    var originalCapacity = GetOriginalCapacity(hotel.Id, room.Id);
                    
                    if (originalCapacity > 0)
                    {
                        var capacityPercentage = (decimal)availability.AvailableRooms / originalCapacity;

                        // If capacity is below 20%
                        if (capacityPercentage < lowCapacityThreshold)
                        {
                            var adminEmail = _hotelAdminEmails.GetValueOrDefault(hotel.Id, "admin@hotel.com");

                            var alert = new LowCapacityAlert
                            {
                                HotelId = hotel.Id,
                                HotelName = hotel.Name,
                                RoomId = room.Id,
                                RoomType = room.RoomType,
                                Date = checkStartDate,
                                TotalCapacity = originalCapacity,
                                AvailableRooms = availability.AvailableRooms,
                                CapacityPercentage = capacityPercentage * 100,
                                AdminEmail = adminEmail
                            };

                            alerts.Add(alert);
                        }
                    }
                }
            }
        }

        // Create notifications for low capacity alerts
        foreach (var alert in alerts)
        {
            await CreateLowCapacityAlertAsync(alert);
        }

        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Low capacity check complete. Found {alerts.Count} alerts.");

        await Task.CompletedTask;
    }

    public async Task ProcessNewReservationsAsync()
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Processing new reservations...");

        // Get bookings created in the last 24 hours that haven't been notified
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        
        var newBookings = _bookings
            .Where(b => 
                b.BookingDate >= cutoffTime && 
                b.Status == BookingStatus.Confirmed &&
                !_notifications.Any(n => 
                    n.BookingId == b.Id && 
                    n.Type == NotificationType.BookingConfirmation &&
                    n.Status == NotificationStatus.Sent))
            .ToList();

        foreach (var booking in newBookings)
        {
            await CreateBookingConfirmationNotificationAsync(booking);
        }

        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Processed {newBookings.Count} new reservations.");

        // Process the notification queue
        await ProcessNotificationQueueAsync();
    }

    #endregion

    #region Notification Processing

    public async Task ProcessNotificationQueueAsync()
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Processing notification queue...");

        var processedCount = 0;
        var failedCount = 0;

        while (_notificationQueue.TryDequeue(out var notification))
        {
            var success = await SendNotificationAsync(notification);
            
            if (success)
            {
                processedCount++;
            }
            else
            {
                failedCount++;
                
                // Retry logic (max 3 retries)
                if (notification.RetryCount < 3)
                {
                    notification.RetryCount++;
                    notification.Status = NotificationStatus.Retrying;
                    _notificationQueue.Enqueue(notification);
                }
            }
        }

        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Queue processing complete. Sent: {processedCount}, Failed: {failedCount}");

        await Task.CompletedTask;
    }

    public async Task<bool> SendNotificationAsync(Notification notification)
    {
        try
        {
            // In production, integrate with email service (SendGrid, AWS SES, etc.)
            // For now, simulate sending
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Sending notification:");
            Console.WriteLine($"  To: {notification.RecipientEmail}");
            Console.WriteLine($"  Type: {notification.Type}");
            Console.WriteLine($"  Subject: {notification.Title}");
            Console.WriteLine($"  Message: {notification.Message.Substring(0, Math.Min(100, notification.Message.Length))}...");

            // Simulate email sending delay
            await Task.Delay(100);

            // Update notification status
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;

            return true;
        }
        catch (Exception ex)
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Failed to send notification: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Notification Retrieval

    public async Task<IEnumerable<NotificationResponse>> GetHotelNotificationsAsync(int hotelId)
    {
        var notifications = _notifications
            .Where(n => n.HotelId == hotelId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(MapToResponse);

        return await Task.FromResult(notifications);
    }

    public async Task<IEnumerable<NotificationResponse>> GetPendingNotificationsAsync()
    {
        var notifications = _notifications
            .Where(n => n.Status == NotificationStatus.Pending || n.Status == NotificationStatus.Retrying)
            .OrderBy(n => n.CreatedAt)
            .Select(MapToResponse);

        return await Task.FromResult(notifications);
    }

    public async Task<NotificationResponse?> GetNotificationByIdAsync(int notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        return await Task.FromResult(notification != null ? MapToResponse(notification) : null);
    }

    #endregion

    #region Helper Methods

    private int GetOriginalCapacity(int hotelId, int roomId)
    {
        // In production, store original capacity in database
        // For demo, use a hardcoded value or estimate from max availability
        var maxAvailability = _availabilities
            .Where(a => a.HotelId == hotelId && a.RoomId == roomId)
            .OrderByDescending(a => a.AvailableRooms)
            .FirstOrDefault();

        return maxAvailability?.AvailableRooms ?? 10; // Default to 10 if not found
    }

    private string GenerateBookingConfirmationMessage(Booking booking, Hotel? hotel, Room? room)
    {
        var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
        
        return $@"Dear {booking.GuestName},

Thank you for choosing {hotel?.Name ?? "our hotel"}!

Your booking has been confirmed.

Booking Details:
- Booking Reference: {booking.BookingReference}
- Hotel: {hotel?.Name ?? "N/A"}
- Location: {hotel?.Location ?? "N/A"}
- Room Type: {room?.RoomType ?? "N/A"}
- Check-in Date: {booking.CheckInDate:MMMM dd, yyyy}
- Check-out Date: {booking.CheckOutDate:MMMM dd, yyyy}
- Number of Nights: {numberOfNights}
- Number of Rooms: {booking.NumberOfRooms}
- Number of Guests: {booking.NumberOfGuests}
- Total Amount: ${booking.TotalPrice:F2}

Special Requests: {booking.SpecialRequests ?? "None"}

We look forward to welcoming you!

If you have any questions, please contact us at {booking.GuestEmail}.

Best regards,
{hotel?.Name ?? "Hotel"} Team";
    }

    private string GenerateBookingCancellationMessage(Booking booking, Hotel? hotel)
    {
        return $@"Dear {booking.GuestName},

Your booking has been cancelled as requested.

Cancelled Booking Details:
- Booking Reference: {booking.BookingReference}
- Hotel: {hotel?.Name ?? "N/A"}
- Check-in Date: {booking.CheckInDate:MMMM dd, yyyy}
- Check-out Date: {booking.CheckOutDate:MMMM dd, yyyy}
- Cancellation Date: {booking.CancellationDate?.ToString("MMMM dd, yyyy") ?? "N/A"}
- Reason: {booking.CancellationReason ?? "Not specified"}

Your refund (if applicable) will be processed within 5-7 business days.

We hope to serve you in the future!

Best regards,
{hotel?.Name ?? "Hotel"} Team";
    }

    private string GenerateLowCapacityMessage(LowCapacityAlert alert)
    {
        return $@"Low Capacity Alert

Hotel: {alert.HotelName}
Room Type: {alert.RoomType}
Date: {alert.Date:MMMM dd, yyyy}

Current Status:
- Total Capacity: {alert.TotalCapacity} rooms
- Available Rooms: {alert.AvailableRooms}
- Capacity Percentage: {alert.CapacityPercentage:F2}%" + @"
                -  Возможно, что цена завышена
                -  Возможно, что не хватает витринного контроля
                -  Возможно, что необходимо проверить корректность работы менеджеров
                -  Возможно, что необходимо проанализировать дополнительные причины

⚠️ WARNING: Available capacity has fallen below 20%!

Action Required:
1. Review pricing strategy for this date
2. Consider increasing rates if demand is high
3. Check for any overbooking issues
4. Verify room maintenance schedule

Please log in to the admin portal to take action.

This is an automated alert from the Hotel Management System.";
    }

    private NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            NotificationId = notification.Id,
            Type = notification.Type.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            RecipientEmail = notification.RecipientEmail,
            Status = notification.Status.ToString(),
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt
        };
    }

    #endregion
}
