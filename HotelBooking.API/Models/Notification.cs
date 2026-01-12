namespace HotelBookingAPI.Models;

public class Notification
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public int? UserId { get; set; }
    public int? HotelId { get; set; }
    public int? BookingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum NotificationType
{
    BookingConfirmation,
    BookingCancellation,
    LowCapacityAlert,
    BookingReminder,
    CheckInReminder,
    SystemAlert
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    Retrying
}
