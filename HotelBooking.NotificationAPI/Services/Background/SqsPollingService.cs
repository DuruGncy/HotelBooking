using HotelBooking.NotificationAPI.Services.Interfaces;

namespace HotelBooking.NotificationAPI.Services.Background;

public class SqsPollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SqsPollingService> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

    public SqsPollingService(IServiceProvider serviceProvider, ILogger<SqsPollingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SQS Polling Service started, polling every {Seconds} seconds.", _pollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // Process messages from SQS queue
                await notificationService.ProcessNotificationQueueAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while polling SQS");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
}
