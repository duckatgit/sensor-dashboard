using Sensor_Api.Data;

namespace Sensor_Api.HostedServices
{
    public class DataCleanupHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DataCleanupHostedService> _logger;

        public DataCleanupHostedService(IServiceScopeFactory scopeFactory, ILogger<DataCleanupHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SensorDbContext>();

                var cutoff = DateTime.UtcNow.AddHours(-24);
                var oldData = context.SensorReadings.Where(r => r.Timestamp < cutoff);

                context.SensorReadings.RemoveRange(oldData);
                await context.SaveChangesAsync();

                _logger.LogInformation("Old readings (older than 24h) purged at {Time}", DateTime.UtcNow);

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // run hourly
            }
        }
    }
}