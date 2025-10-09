using Microsoft.EntityFrameworkCore;
using Sensor_Api.Data;

namespace Sensor_Api.HostedServices
{
    public class PurgeHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PurgeHostedService> _logger;

        public PurgeHostedService(IServiceScopeFactory scopeFactory, ILogger<PurgeHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PurgeHostedService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<SensorDbContext>();

                    var cutoff = DateTime.UtcNow.AddHours(-24); // keep last 10 min
                    var oldReadings = await context.SensorReadings
                        .Where(r => r.Timestamp < cutoff)
                        .ToListAsync(stoppingToken);

                    if (oldReadings.Any())
                    {
                        context.SensorReadings.RemoveRange(oldReadings);
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation($"Purged {oldReadings.Count} old records older than {cutoff}.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during purge operation");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // every 1 minute
            }
        }
    }
}
