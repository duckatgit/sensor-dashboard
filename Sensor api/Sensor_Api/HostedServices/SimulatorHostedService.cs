using Microsoft.AspNetCore.SignalR;
using Sensor_Api.Entities;
using Sensor_Api.Hubs;
using Sensor_Api.Services;

namespace Sensor_Api.HostedServices
{
    public class SimulatorHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<SensorHub> _hubContext;

        public SimulatorHostedService(IServiceScopeFactory scopeFactory, IHubContext<SensorHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISensorService>();
            var rand = new Random();
            while (!stoppingToken.IsCancellationRequested)
            {
                var readings = new List<SensorReading>(1000);
                for (int i = 0; i < 1000; i++)
                {
                    readings.Add(new SensorReading
                    {
                        SensorId = $"Sensor-{rand.Next(1, 5)}",
                        Value = rand.NextDouble() * 1000,
                        Timestamp = DateTime.UtcNow
                    });
                }
                await service.AddReadingsBatchAsync(readings);

                var groupedStats = readings
                    .GroupBy(r => r.SensorId)
                    .Select(g => new {
                        SensorId = g.Key,
                        Average = g.Average(x => x.Value),
                        Min = g.Min(x => x.Value),
                        Max = g.Max(x => x.Value),
                        Count = g.Count(),
                        Timestamp = DateTime.UtcNow
                    });

                await _hubContext.Clients.All.SendAsync("ReceiveBatchStats", groupedStats, stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }

    }
}
