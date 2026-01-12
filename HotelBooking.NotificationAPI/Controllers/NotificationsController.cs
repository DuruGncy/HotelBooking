using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBooking.NotificationAPI.Models.DTOs;
using HotelBooking.NotificationAPI.Services.Interfaces;
using HotelBooking.NotificationAPI.Models;

namespace HotelBooking.NotificationAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new
        {
            service = "Notification API",
            status = "healthy",
            timestamp = DateTime.UtcNow,
            description = "Notification service for managing hotel booking notifications"
        });
    }

    /// <summary>
    /// Receive booking confirmation request from ClientAPI
    /// </summary>
    [HttpPost("booking-confirmation")]
    [AllowAnonymous] // Called by internal services
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ReceiveBookingConfirmation([FromBody] BookingNotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Received booking confirmation request for booking {BookingId}", request.BookingId);

            // Create a Booking object from the request
            var booking = new Booking
            {
                Id = request.BookingId,
                HotelId = request.HotelId,
                RoomId = request.RoomId,
                GuestName = request.GuestName,
                GuestEmail = request.GuestEmail,
                BookingReference = request.BookingReference,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                NumberOfRooms = request.NumberOfRooms,
                NumberOfGuests = request.NumberOfGuests,
                TotalPrice = request.TotalPrice,
                SpecialRequests = request.SpecialRequests,
                Status = BookingStatus.Confirmed,
                BookingDate = DateTime.UtcNow
            };

            // Create and queue the notification
            await _notificationService.CreateBookingConfirmationNotificationAsync(booking);

            return Accepted(new
            {
                message = "Booking confirmation notification queued successfully",
                bookingId = request.BookingId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing booking confirmation for booking {BookingId}", request.BookingId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Receive booking cancellation request from ClientAPI
    /// </summary>
    [HttpPost("booking-cancellation")]
    [AllowAnonymous] // Called by internal services
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ReceiveBookingCancellation([FromBody] BookingCancellationRequest request)
    {
        try
        {
            _logger.LogInformation("Received booking cancellation request for booking {BookingId}", request.BookingId);

            // Create a Booking object from the request
            var booking = new Booking
            {
                Id = request.BookingId,
                HotelId = request.HotelId,
                GuestName = request.GuestName,
                GuestEmail = request.GuestEmail,
                BookingReference = request.BookingReference,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                CancellationDate = request.CancellationDate,
                CancellationReason = request.CancellationReason,
                Status = BookingStatus.Cancelled
            };

            // Create and queue the notification
            await _notificationService.CreateBookingCancellationNotificationAsync(booking);

            return Accepted(new
            {
                message = "Booking cancellation notification queued successfully",
                bookingId = request.BookingId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing booking cancellation for booking {BookingId}", request.BookingId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all notifications for a specific hotel (Admin only)
    /// </summary>
    [HttpGet("hotel/{hotelId}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetHotelNotifications(int hotelId)
    {
        var notifications = await _notificationService.GetHotelNotificationsAsync(hotelId);
        return Ok(notifications);
    }

    /// <summary>
    /// Get all pending notifications (Admin only)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetPendingNotifications()
    {
        var notifications = await _notificationService.GetPendingNotificationsAsync();
        return Ok(notifications);
    }

    /// <summary>
    /// Get notification by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<NotificationResponse>> GetNotificationById(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        
        if (notification == null)
        {
            return NotFound(new { error = $"Notification with ID {id} not found" });
        }

        return Ok(notification);
    }

    /// <summary>
    /// Manually trigger low capacity check (Admin only)
    /// </summary>
    [HttpPost("trigger/low-capacity-check")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<object>> TriggerLowCapacityCheck()
    {
        await _notificationService.CheckLowCapacityAndNotifyAsync();
        
        return Ok(new
        {
            message = "Low capacity check triggered successfully",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Manually trigger reservation processing (Admin only)
    /// </summary>
    [HttpPost("trigger/process-reservations")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<object>> TriggerReservationProcessing()
    {
        await _notificationService.ProcessNewReservationsAsync();
        
        return Ok(new
        {
            message = "Reservation processing triggered successfully",
            timestamp = DateTime.UtcNow
        });
    }
}
