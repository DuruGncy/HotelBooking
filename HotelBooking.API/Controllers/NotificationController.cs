using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBookingAPI.Models.DTOs;
using HotelBookingAPI.Services.Interfaces;

namespace HotelBookingAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
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

    /// <summary>
    /// Process notification queue manually (Admin only)
    /// </summary>
    [HttpPost("process-queue")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<object>> ProcessNotificationQueue()
    {
        await _notificationService.ProcessNotificationQueueAsync();
        
        return Ok(new 
        { 
            message = "Notification queue processed successfully",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get notification statistics (Admin only)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<object>> GetNotificationStats()
    {
        var allNotifications = await _notificationService.GetPendingNotificationsAsync();
        var pending = allNotifications.Count();

        return Ok(new
        {
            totalPending = pending,
            timestamp = DateTime.UtcNow
        });
    }
}
