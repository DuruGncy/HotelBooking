using HotelBooking.NotificationAPI.Models;
using HotelBooking.NotificationAPI.Models.DTOs;
using HotelBooking.NotificationAPI.Services.Interfaces;
using HotelBooking.NotificationAPI.Data;
using Microsoft.EntityFrameworkCore;
using NotificationEntity = HotelBooking.NotificationAPI.Data.Entities.Notification;
using BookingEntity = HotelBooking.NotificationAPI.Data.Entities.Booking;
using HotelEntity = HotelBooking.NotificationAPI.Data.Entities.Hotel;

namespace HotelBooking.NotificationAPI.Services;

public class NotificationService : INotificationService
{
    private readonly ISqsService _sqsService;
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationDbContext _context;

    public NotificationService(ISqsService sqsService, ILogger<NotificationService> logger, NotificationDbContext context)
    {
        _sqsService = sqsService;
        _logger = logger;
        _context = context;
    }

    #region Notification Creation

    public async Task<Notification> CreateBookingConfirmationNotificationAsync(Models.Booking booking)
    {
        try
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == booking.HotelId);

            var notification = new NotificationEntity
            {
                NotificationType = "BookingConfirmation",
                Subject = "Booking Confirmation",
                Message = GenerateBookingConfirmationMessage(booking, hotel),
                RecipientEmail = booking.GuestEmail,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                ScheduledFor = DateTime.UtcNow,
                RelatedBookingId = booking.Id,
                RelatedHotelId = booking.HotelId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            // Send to SQS FIFO queue instead of in-memory queue
            var modelToSend = MapToModel(notification);
            var success = await _sqsService.SendNotificationToQueueAsync(modelToSend);
            
            if (success)
            {
                _logger.LogInformation("Booking confirmation notification {NotificationId} sent to SQS queue", notification.Id);
            }
            else
            {
                _logger.LogWarning("Failed to send booking confirmation notification {NotificationId} to SQS queue", notification.Id);
            }

            return MapToModel(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking confirmation notification");
            throw;
        }
    }

    public async Task<Notification> CreateBookingCancellationNotificationAsync(Models.Booking booking)
    {
        try
        {
            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == booking.HotelId);

            var notification = new NotificationEntity
            {
                NotificationType = "BookingCancellation",
                Subject = "Booking Cancellation Confirmation",
                Message = GenerateBookingCancellationMessage(booking, hotel),
                RecipientEmail = booking.GuestEmail,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                ScheduledFor = DateTime.UtcNow,
                RelatedBookingId = booking.Id,
                RelatedHotelId = booking.HotelId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            // Send to SQS FIFO queue
            var modelToSend = MapToModel(notification);
            var success = await _sqsService.SendNotificationToQueueAsync(modelToSend);
            
            if (success)
            {
                _logger.LogInformation("Booking cancellation notification {NotificationId} sent to SQS queue", notification.Id);
            }
            else
            {
                _logger.LogWarning("Failed to send booking cancellation notification {NotificationId} to SQS queue", notification.Id);
            }

            return MapToModel(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking cancellation notification");
            throw;
        }
    }

    public async Task<Notification> CreateLowCapacityAlertAsync(LowCapacityAlert alert)
    {
        try
        {
            var notification = new NotificationEntity
            {
                NotificationType = "LowCapacityAlert",
                Subject = $"Low Capacity Alert - {alert.HotelName}",
                Message = GenerateLowCapacityMessage(alert),
                RecipientEmail = alert.AdminEmail,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                ScheduledFor = DateTime.UtcNow,
                RelatedHotelId = alert.HotelId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            
            // Send to SQS FIFO queue
            var modelToSend = MapToModel(notification);
            var success = await _sqsService.SendNotificationToQueueAsync(modelToSend);
            
            if (success)
            {
                _logger.LogInformation("Low capacity alert {NotificationId} sent to SQS queue", notification.Id);
            }
            else
            {
                _logger.LogWarning("Failed to send low capacity alert {NotificationId} to SQS queue", notification.Id);
            }

            return MapToModel(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating low capacity alert notification");
            throw;
        }
    }

    #endregion

    #region Scheduled Tasks

    public async Task CheckLowCapacityAndNotifyAsync()
    {
        try
        {
            _logger.LogInformation("Running low capacity check...");

            var startDate = DateTime.UtcNow.Date.AddDays(1); // Tomorrow
            var endDate = startDate.AddMonths(1); // Next month
            var lowCapacityThreshold = 0.20m; // 20%

            var alerts = new List<LowCapacityAlert>();

            // Check each hotel's capacity
            var hotels = await _context.Hotels.ToListAsync();
            
            foreach (var hotel in hotels)
            {
                // For low capacity alerts, we're just looking at hotels with bookings
                var hotelBookings = await _context.Bookings
                    .Where(b => b.HotelId == hotel.Id)
                    .ToListAsync();

                if (hotelBookings.Any())
                {
                    var alert = new LowCapacityAlert
                    {
                        HotelId = hotel.Id,
                        HotelName = hotel.Name,
                        RoomId = 0,
                        RoomType = "General",
                        Date = startDate,
                        TotalCapacity = hotelBookings.Count(),
                        AvailableRooms = 0,
                        CapacityPercentage = 0,
                        AdminEmail = GetHotelAdminEmail(hotel.Id)
                    };

                    // Only add if capacity is low
                    if (alert.AvailableRooms < lowCapacityThreshold * alert.TotalCapacity)
                    {
                        alerts.Add(alert);
                    }
                }
            }

            // Create notifications for low capacity alerts
            foreach (var alert in alerts)
            {
                await CreateLowCapacityAlertAsync(alert);
            }

            _logger.LogInformation("Low capacity check complete. Found {AlertCount} alerts.", alerts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during low capacity check");
        }
    }

    public async Task ProcessNewReservationsAsync()
    {
        try
        {
            _logger.LogInformation("Processing new reservations...");

            // Get bookings created in the last 24 hours that haven't been notified
            var cutoffTime = DateTime.UtcNow.AddHours(-24);
            
            var newBookings = await _context.Bookings
                .Where(b => 
                    b.BookingDate >= cutoffTime && 
                    b.Status == "Confirmed" &&
                    !_context.Notifications.Any(n => 
                        n.RelatedBookingId == b.Id && 
                        n.NotificationType == "BookingConfirmation" &&
                        n.Status == "Sent"))
                .ToListAsync();

            foreach (var booking in newBookings)
            {
                await CreateBookingConfirmationNotificationAsync(MapToModel(booking));
            }

            _logger.LogInformation("Processed {BookingCount} new reservations.", newBookings.Count);

            // Process the notification queue from SQS
            await ProcessNotificationQueueAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing new reservations");
        }
    }

    #endregion

    #region Notification Processing

    public async Task ProcessNotificationQueueAsync()
    {
        try
        {
            _logger.LogInformation("Processing notification queue from SQS...");

            var processedCount = 0;
            var failedCount = 0;

            // Get queue message count
            var queueCount = await _sqsService.GetQueueMessageCountAsync();
            _logger.LogInformation("Approximate messages in queue: {QueueCount}", queueCount);

            // Receive messages from SQS (process in batches)
            var notifications = await _sqsService.ReceiveNotificationsFromQueueAsync(10);

            foreach (var notification in notifications)
            {
                var success = await SendNotificationAsync(notification);
                
                if (success)
                {
                    processedCount++;
                    
                    // Delete the message from queue after successful processing
                    if (notification.Metadata != null && 
                        notification.Metadata.TryGetValue("ReceiptHandle", out var receiptHandle))
                    {
                        await _sqsService.DeleteMessageAsync(receiptHandle);
                        _logger.LogInformation("Deleted message for notification {NotificationId} from SQS", notification.Id);
                    }
                }
                else
                {
                    failedCount++;
                    
                    // For failed messages, don't delete from queue
                    // SQS will automatically retry based on visibility timeout and dead letter queue config
                    _logger.LogWarning("Failed to process notification {NotificationId}. Message will remain in queue for retry.", notification.Id);
                }
            }

            _logger.LogInformation("Queue processing complete. Sent: {SentCount}, Failed: {FailedCount}", processedCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification queue");
        }
    }

    public async Task<bool> SendNotificationAsync(Notification notification)
    {
        try
        {
            // In production, integrate with email service (SendGrid, AWS SES, etc.)
            // For now, simulate sending
            _logger.LogInformation("Sending notification:");
            _logger.LogInformation("  To: {Email}", notification.RecipientEmail);
            _logger.LogInformation("  Type: {Type}", notification.Title);
            _logger.LogInformation("  Subject: {Title}", notification.Title);
            _logger.LogInformation("  Message: {Message}", notification.Message.Substring(0, Math.Min(100, notification.Message.Length)) + "...");

            // Simulate email sending delay
            await Task.Delay(100);

            // Update notification status in database by querying the entity
            var entity = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notification.Id);
            if (entity != null)
            {
                entity.Status = "Sent";
                entity.SentAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                
                _context.Notifications.Update(entity);
                await _context.SaveChangesAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification {NotificationId}", notification.Id);
            return false;
        }
    }

    #endregion

    #region Notification Retrieval

    public async Task<IEnumerable<NotificationResponse>> GetHotelNotificationsAsync(int hotelId)
    {
        try
        {
            var notifications = await _context.Notifications
                .Where(n => n.RelatedHotelId == hotelId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(n => new NotificationResponse
            {
                NotificationId = n.Id,
                Type = n.NotificationType,
                Title = n.Subject,
                Message = n.Message,
                RecipientEmail = n.RecipientEmail,
                Status = n.Status,
                CreatedAt = n.CreatedAt,
                SentAt = n.SentAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hotel notifications for hotel {HotelId}", hotelId);
            return Enumerable.Empty<NotificationResponse>();
        }
    }

    public async Task<IEnumerable<NotificationResponse>> GetPendingNotificationsAsync()
    {
        try
        {
            var notifications = await _context.Notifications
                .Where(n => n.Status == "Pending" || n.Status == "Retrying")
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();

            return notifications.Select(n => new NotificationResponse
            {
                NotificationId = n.Id,
                Type = n.NotificationType,
                Title = n.Subject,
                Message = n.Message,
                RecipientEmail = n.RecipientEmail,
                Status = n.Status,
                CreatedAt = n.CreatedAt,
                SentAt = n.SentAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending notifications");
            return Enumerable.Empty<NotificationResponse>();
        }
    }

    public async Task<NotificationResponse?> GetNotificationByIdAsync(int notificationId)
    {
        try
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notification == null)
                return null;

            return new NotificationResponse
            {
                NotificationId = notification.Id,
                Type = notification.NotificationType,
                Title = notification.Subject,
                Message = notification.Message,
                RecipientEmail = notification.RecipientEmail,
                Status = notification.Status,
                CreatedAt = notification.CreatedAt,
                SentAt = notification.SentAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification {NotificationId}", notificationId);
            return null;
        }
    }

    #endregion

    #region Helper Methods

    private string GetHotelAdminEmail(int hotelId)
    {
        // In production, retrieve from database
        // For now, use a default or hardcoded value
        return "admin@hotel.com";
    }

    private string GenerateBookingConfirmationMessage(Models.Booking booking, HotelEntity? hotel)
    {
        var numberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
        
        return $@"Dear {booking.GuestName},

Thank you for choosing {hotel?.Name ?? "our hotel"}!

Your booking has been confirmed.

Booking Details:
- Booking Reference: {booking.BookingReference}
- Hotel: {hotel?.Name ?? "N/A"}
- Location: {hotel?.Location ?? "N/A"}
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

    private string GenerateBookingCancellationMessage(Models.Booking booking, HotelEntity? hotel)
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
⚠️ WARNING: Available capacity has fallen below 20%!

Action Required:
1. Review pricing strategy for this date
2. Consider increasing rates if demand is high
3. Check for any overbooking issues
4. Verify room maintenance schedule

Please log in to the admin portal to take action.

This is an automated alert from the Hotel Management System.";
    }

    private Notification MapToModel(NotificationEntity entity)
    {
        return new Notification
        {
            Id = entity.Id,
            Type = entity.NotificationType switch
            {
                "BookingConfirmation" => NotificationType.BookingConfirmation,
                "BookingCancellation" => NotificationType.BookingCancellation,
                "LowCapacityAlert" => NotificationType.LowCapacityAlert,
                _ => NotificationType.SystemAlert
            },
            Title = entity.Subject,
            Message = entity.Message,
            RecipientEmail = entity.RecipientEmail,
            Status = entity.Status switch
            {
                "Pending" => NotificationStatus.Pending,
                "Sent" => NotificationStatus.Sent,
                "Failed" => NotificationStatus.Failed,
                "Retrying" => NotificationStatus.Retrying,
                _ => NotificationStatus.Pending
            },
            CreatedAt = entity.CreatedAt,
            SentAt = entity.SentAt,
            HotelId = entity.RelatedHotelId,
            BookingId = entity.RelatedBookingId
        };
    }

    private Models.Booking MapToModel(BookingEntity entity)
    {
        return new Models.Booking
        {
            Id = entity.Id,
            HotelId = entity.HotelId,
            RoomId = entity.RoomId,
            BookingReference = entity.BookingReference,
            CheckInDate = entity.CheckInDate,
            CheckOutDate = entity.CheckOutDate,
            NumberOfRooms = 1, // Default value since entity doesn't track this
            NumberOfGuests = 1, // Default value since entity doesn't track this
            TotalPrice = 0, // Default value since entity doesn't track this
            Status = entity.Status switch
            {
                "Confirmed" => BookingStatus.Confirmed,
                "Pending" => BookingStatus.Pending,
                "CheckedIn" => BookingStatus.CheckedIn,
                "CheckedOut" => BookingStatus.CheckedOut,
                "Cancelled" => BookingStatus.Cancelled,
                _ => BookingStatus.Pending
            },
            BookingDate = entity.BookingDate,
            GuestName = entity.GuestName,
            GuestEmail = entity.GuestEmail,
            GuestPhone = null, // Entity doesn't have this
            SpecialRequests = null, // Entity doesn't have this
            CancellationDate = null, // Entity doesn't have this
            CancellationReason = null // Entity doesn't have this
        };
    }

    #endregion
}
