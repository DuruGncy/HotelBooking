using HotelBooking.NotificationAPI.Models;

namespace HotelBooking.NotificationAPI.Services.Interfaces;

public interface ISqsService
{
    /// <summary>
    /// Send a notification message to the SQS FIFO queue
    /// </summary>
    Task<bool> SendNotificationToQueueAsync(Notification notification);

    /// <summary>
    /// Receive messages from the SQS FIFO queue
    /// </summary>
    Task<List<Notification>> ReceiveNotificationsFromQueueAsync(int maxMessages = 10);

    /// <summary>
    /// Delete a message from the queue after processing
    /// </summary>
    Task<bool> DeleteMessageAsync(string receiptHandle);

    /// <summary>
    /// Get the approximate number of messages in the queue
    /// </summary>
    Task<int> GetQueueMessageCountAsync();
}
