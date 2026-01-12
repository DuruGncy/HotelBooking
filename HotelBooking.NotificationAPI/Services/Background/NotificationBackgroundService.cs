using HotelBooking.NotificationAPI.Services.Interfaces;

namespace HotelBooking.NotificationAPI.Services.Background;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;
    private Timer? _lowCapacityTimer;
    private Timer? _reservationTimer;

    public NotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Background Service is starting.");

        // Schedule low capacity check - runs daily at 2 AM
        ScheduleLowCapacityCheck(stoppingToken);

        // Schedule reservation processing - runs every hour
        ScheduleReservationProcessing(stoppingToken);

        await Task.CompletedTask;
    }

    private void ScheduleLowCapacityCheck(CancellationToken stoppingToken)
    {
        // Calculate time until next 2 AM
        var now = DateTime.Now;
        var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0);
        
        if (now > scheduledTime)
        {
            scheduledTime = scheduledTime.AddDays(1);
        }

        var timeUntilScheduled = scheduledTime - now;
        
        _logger.LogInformation($"Low capacity check scheduled for {scheduledTime:yyyy-MM-dd HH:mm:ss}");

        _lowCapacityTimer = new Timer(
            async _ => await CheckLowCapacityAsync(stoppingToken),
            null,
            timeUntilScheduled,
            TimeSpan.FromDays(1)); // Repeat daily
    }

    private void ScheduleReservationProcessing(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reservation processing will run every hour.");

        _reservationTimer = new Timer(
            async _ => await ProcessReservationsAsync(stoppingToken),
            null,
            TimeSpan.Zero, // Start immediately
            TimeSpan.FromHours(1)); // Repeat hourly
    }

    private async Task CheckLowCapacityAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
            return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            _logger.LogInformation("Running nightly low capacity check...");
            await notificationService.CheckLowCapacityAndNotifyAsync();
            _logger.LogInformation("Low capacity check completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during low capacity check.");
        }
    }

    private async Task ProcessReservationsAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
            return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            _logger.LogInformation("Processing new reservations...");
            await notificationService.ProcessNewReservationsAsync();
            _logger.LogInformation("Reservation processing completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during reservation processing.");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Background Service is stopping.");

        _lowCapacityTimer?.Change(Timeout.Infinite, 0);
        _reservationTimer?.Change(Timeout.Infinite, 0);

        await base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _lowCapacityTimer?.Dispose();
        _reservationTimer?.Dispose();
        base.Dispose();
    }
}
