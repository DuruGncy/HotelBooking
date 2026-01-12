using Amazon.SQS;
using Amazon.SQS.Model;
using HotelBooking.NotificationAPI.Models;
using HotelBooking.NotificationAPI.Services.Interfaces;
using System.Text.Json;

namespace HotelBooking.NotificationAPI.Services;

public class SqsService : ISqsService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;
    private readonly ILogger<SqsService> _logger;

    public SqsService(IAmazonSQS sqsClient, IConfiguration configuration, ILogger<SqsService> logger)
    {
        _sqsClient = sqsClient;
        _queueUrl = configuration["AWS:SQS:QueueUrl"] ?? throw new ArgumentNullException("AWS:SQS:QueueUrl is not configured");
        _logger = logger;
    }

    public async Task<bool> SendNotificationToQueueAsync(Notification notification)
    {
        try
        {
            // Serialize the notification to JSON
            var messageBody = JsonSerializer.Serialize(notification);

            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = messageBody,
                
                // FIFO specific attributes
                MessageGroupId = notification.HotelId?.ToString() ?? "default", // Group by hotel for FIFO ordering
                MessageDeduplicationId = $"{notification.Id}-{Guid.NewGuid()}", // Unique ID to prevent duplicates
                
                // Add message attributes for filtering/routing
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "NotificationType", new MessageAttributeValue { DataType = "String", StringValue = notification.Type.ToString() } },
                    { "NotificationId", new MessageAttributeValue { DataType = "Number", StringValue = notification.Id.ToString() } },
                    { "HotelId", new MessageAttributeValue { DataType = "Number", StringValue = notification.HotelId?.ToString() ?? "0" } },
                    { "Priority", new MessageAttributeValue { DataType = "String", StringValue = GetPriority(notification.Type) } }
                }
            };

            var response = await _sqsClient.SendMessageAsync(sendMessageRequest);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation(
                    "Successfully sent notification {NotificationId} to SQS queue. MessageId: {MessageId}",
                    notification.Id, response.MessageId);
                return true;
            }

            _logger.LogWarning(
                "Failed to send notification {NotificationId} to SQS. Status: {StatusCode}",
                notification.Id, response.HttpStatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending notification {NotificationId} to SQS queue", 
                notification.Id);
            return false;
        }
    }

    public async Task<List<Notification>> ReceiveNotificationsFromQueueAsync(int maxMessages = 10)
    {
        try
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = Math.Min(maxMessages, 10), // SQS max is 10
                WaitTimeSeconds = 20, // Long polling for better efficiency
                MessageAttributeNames = new List<string> { "All" },
                AttributeNames = new List<string> { "All" }
            };

            var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);

            var notifications = new List<Notification>();

            foreach (var message in response.Messages)
            {
                try
                {
                    // Deserialize the notification
                    var notification = JsonSerializer.Deserialize<Notification>(message.Body);
                    
                    if (notification != null)
                    {
                        // Store the receipt handle for deletion later
                        notification.Metadata ??= new Dictionary<string, string>();
                        notification.Metadata["ReceiptHandle"] = message.ReceiptHandle;
                        notification.Metadata["MessageId"] = message.MessageId;
                        
                        notifications.Add(notification);

                        _logger.LogInformation(
                            "Received notification {NotificationId} from SQS. MessageId: {MessageId}",
                            notification.Id, message.MessageId);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, 
                        "Failed to deserialize message {MessageId} from SQS", 
                        message.MessageId);
                }
            }

            return notifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving messages from SQS queue");
            return new List<Notification>();
        }
    }

    public async Task<bool> DeleteMessageAsync(string receiptHandle)
    {
        try
        {
            var deleteMessageRequest = new DeleteMessageRequest
            {
                QueueUrl = _queueUrl,
                ReceiptHandle = receiptHandle
            };

            var response = await _sqsClient.DeleteMessageAsync(deleteMessageRequest);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Successfully deleted message from SQS queue");
                return true;
            }

            _logger.LogWarning("Failed to delete message from SQS. Status: {StatusCode}", response.HttpStatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message from SQS queue");
            return false;
        }
    }

    public async Task<int> GetQueueMessageCountAsync()
    {
        try
        {
            var getAttributesRequest = new GetQueueAttributesRequest
            {
                QueueUrl = _queueUrl,
                AttributeNames = new List<string> 
                { 
                    "ApproximateNumberOfMessages",
                    "ApproximateNumberOfMessagesNotVisible",
                    "ApproximateNumberOfMessagesDelayed"
                }
            };

            var response = await _sqsClient.GetQueueAttributesAsync(getAttributesRequest);

            var totalMessages = 0;
            
            if (response.ApproximateNumberOfMessages.HasValue)
                totalMessages += response.ApproximateNumberOfMessages.Value;
            
            if (response.ApproximateNumberOfMessagesNotVisible.HasValue)
                totalMessages += response.ApproximateNumberOfMessagesNotVisible.Value;
            
            if (response.ApproximateNumberOfMessagesDelayed.HasValue)
                totalMessages += response.ApproximateNumberOfMessagesDelayed.Value;

            return totalMessages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue message count");
            return 0;
        }
    }

    private string GetPriority(NotificationType notificationType)
    {
        return notificationType switch
        {
            NotificationType.LowCapacityAlert => "High",
            NotificationType.BookingConfirmation => "Medium",
            NotificationType.BookingCancellation => "Medium",
            NotificationType.PaymentConfirmation => "High",
            NotificationType.CheckInReminder => "Low",
            NotificationType.CheckOutReminder => "Low",
            _ => "Low"
        };
    }
}
