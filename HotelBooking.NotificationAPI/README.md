# HotelBooking.NotificationAPI

## Overview
The Notification API is a microservice responsible for managing all hotel booking notifications and alerts within the Hotel Booking system.

## Features

### ?? Notification Types
- **Booking Confirmations** - Sent when a guest books a room
- **Booking Cancellations** - Sent when a guest cancels their booking
- **Low Capacity Alerts** - Sent to hotel admins when room availability drops below 20%
- **Booking Reminders** - Future feature for check-in reminders
- **System Alerts** - General system notifications

### ?? Background Services
- **Low Capacity Check** - Runs daily at 2 AM to detect low availability
- **Reservation Processing** - Runs hourly to process new bookings and send confirmations

### ?? Notification Queue
- Asynchronous notification processing
- Automatic retry mechanism (up to 3 attempts)
- Failed notification tracking

## API Endpoints

### Health Check
```http
GET /health
```
Returns the health status of the Notification API.

### Get Hotel Notifications (Admin Only)
```http
GET /api/notifications/hotel/{hotelId}
Authorization: Bearer {jwt-token}
```
Retrieves all notifications for a specific hotel.

### Get Pending Notifications (Admin Only)
```http
GET /api/notifications/pending
Authorization: Bearer {jwt-token}
```
Retrieves all pending notifications in the queue.

### Get Notification by ID (Admin Only)
```http
GET /api/notifications/{id}
Authorization: Bearer {jwt-token}
```
Retrieves a specific notification by ID.

### Trigger Low Capacity Check (Admin Only)
```http
POST /api/notifications/trigger/low-capacity-check
Authorization: Bearer {jwt-token}
```
Manually triggers the low capacity check process.

### Trigger Reservation Processing (Admin Only)
```http
POST /api/notifications/trigger/process-reservations
Authorization: Bearer {jwt-token}
```
Manually triggers the reservation processing.

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "YourDefaultSecretKeyForDevelopmentMinimum32Chars!",
    "Issuer": "HotelBookingAPI",
    "Audience": "HotelBookingAPIUsers"
  }
}
```

## Running the Service

### Standalone
```bash
cd HotelBooking.NotificationAPI
dotnet run
```
The API will be available at `https://localhost:7XXX` (port assigned by Aspire).

### With Aspire
```bash
cd HotelBooking.AppHost
dotnet run
```
View in Aspire Dashboard for monitoring and logs.

## Architecture

### Services
- **INotificationService** - Interface for notification operations
- **NotificationService** - Core notification logic and processing
- **NotificationBackgroundService** - Scheduled task management

### Models
- **Notification** - Core notification entity
- **NotificationResponse** - DTO for API responses
- **LowCapacityAlert** - DTO for capacity alerts
- **Hotel, Room, RoomAvailability, Booking** - Supporting models

### Background Jobs
1. **Low Capacity Check** (Daily @ 2 AM)
   - Scans next month's availability
   - Identifies rooms below 20% capacity
   - Sends alerts to hotel admins

2. **Reservation Processing** (Hourly)
   - Finds bookings from last 24 hours
   - Sends confirmation emails
   - Processes notification queue

## Integration Points

### Email Service (Production)
In production, integrate with email services:
- **SendGrid** - Recommended for transactional emails
- **AWS SES** - For AWS-based deployments
- **SMTP** - For self-hosted solutions

Currently, the service simulates email sending for development.

### Data Sharing
The service accesses shared data:
- Hotels
- Rooms
- Room Availability
- Bookings

**Note:** In production, use a shared database or message queue for data synchronization.

## Authentication & Authorization

All admin endpoints require:
- Valid JWT token in Authorization header
- User role: `ADMIN`

Example:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Development

### Add New Notification Type
1. Update `NotificationType` enum in `Models/Notification.cs`
2. Add creation method in `INotificationService` interface
3. Implement method in `NotificationService`
4. Add message template helper method

### Modify Background Schedule
Edit `NotificationBackgroundService.cs`:
- `ScheduleLowCapacityCheck()` - Change daily schedule
- `ScheduleReservationProcessing()` - Change hourly interval

## Monitoring

### Logs
The service logs:
- Notification creation
- Queue processing
- Email sending attempts
- Background job execution
- Errors and failures

View logs in:
- Console output
- Aspire Dashboard
- Application Insights (production)

## Error Handling

### Retry Logic
- Failed notifications automatically retry (max 3 times)
- Status tracking: Pending ? Retrying ? Sent/Failed

### Error States
- **Pending** - Waiting in queue
- **Sent** - Successfully delivered
- **Failed** - All retry attempts exhausted
- **Retrying** - Currently retrying

## Future Enhancements

- [ ] Real email service integration
- [ ] SMS notifications
- [ ] Push notifications
- [ ] Notification preferences per user
- [ ] Scheduled notifications
- [ ] Template management system
- [ ] Multi-language support
- [ ] Delivery confirmation tracking

## Testing

### Manual Testing
Use Swagger UI at `https://localhost:7XXX/swagger` to test endpoints.

### Trigger Background Jobs Manually
```http
POST /api/notifications/trigger/low-capacity-check
POST /api/notifications/trigger/process-reservations
```

## Dependencies

- ASP.NET Core 9.0
- Microsoft.AspNetCore.Authentication.JwtBearer
- Aspire ServiceDefaults
- Swashbuckle (Swagger)

## Troubleshooting

### Notifications Not Sending
1. Check if NotificationBackgroundService is running
2. Verify data initialization
3. Check logs for errors
4. Ensure booking/hotel data is available

### Authentication Errors
1. Verify JWT settings match across all services
2. Check token expiration
3. Confirm user has ADMIN role

### Background Jobs Not Running
1. Check service startup logs
2. Verify timer configuration
3. Ensure no exceptions in job execution

## Support

For issues or questions:
- Check logs in Aspire Dashboard
- Review service configuration
- Verify authentication settings
- Ensure data is properly initialized
