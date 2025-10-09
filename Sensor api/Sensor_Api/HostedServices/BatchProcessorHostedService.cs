using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sensor_Api.Hubs;
using Sensor_Api.Services;
using Sensor_Api.Utils;

namespace Sensor_Api.HostedServices
{
    public class BatchProcessorHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly ILogger<BatchProcessorHostedService> _logger;
        private readonly CircularBuffer<double> _buffer = new(1000000);

        public BatchProcessorHostedService(
            IServiceScopeFactory scopeFactory,
            IHubContext<SensorHub> hubContext,
            ILogger<BatchProcessorHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BatchProcessorHostedService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<ISensorService>();

                    var readings = await service.GetReadingsBatchAsync(startId: 0, batchSize: 1000);
                    if (!readings.Any())
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        continue;
                    }

                    var welford = new Welford();
                    double min = double.MaxValue;
                    double max = double.MinValue;

                    foreach (var r in readings)
                    {
                        _buffer.Add(r.Value);
                        welford.AddSample(r.Value);
                        min = Math.Min(min, r.Value);
                        max = Math.Max(max, r.Value);

                        if (r.Value > 900 || r.Value < 10)
                        {
                            await _hubContext.Clients.All.SendAsync("Alert", new
                            {
                                SensorId = r.SensorId,
                                Value = r.Value,
                                Timestamp = r.Timestamp,
                                Message = $"⚠️ Sensor {r.SensorId} abnormal reading: {r.Value:F2}"
                            }, cancellationToken: stoppingToken);
                        }
                    }

                    var stats = new
                    {
                        Count = welford.Count,
                        Mean = welford.Mean,
                        StdDev = welford.StandardDeviation,
                        Min = min,
                        Max = max,
                        Timestamp = DateTime.UtcNow
                    };

                    await _hubContext.Clients.All.SendAsync("ReceiveBatchStats", stats, cancellationToken: stoppingToken);
                    if (max > 90)
                    {
                        await _hubContext.Clients.All.SendAsync("Alert", new
                        {
                            MaxValue = max,
                            Timestamp = DateTime.UtcNow,
                            Message = $"⚠️ Max value exceeded : {max:F2}"
                        }, cancellationToken: stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch processing");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

    }
}

