using Sensor_Api.Entities;
using Sensor_Api.Repositories;

namespace Sensor_Api.Services
{
    public class SensorService : ISensorService
    {
        private readonly ISensorRepository _repo;
        private readonly Random _rand = new();

        public SensorService(ISensorRepository repo)
        {
            _repo = repo;
        }

        public async Task GenerateReadingAsync()
        {
            var reading = new SensorReading
            {
                SensorId = "Sensor-" + _rand.Next(1, 5),
                Value = _rand.NextDouble() * 1000,
                Timestamp = DateTime.UtcNow
            };

            await _repo.AddReadingAsync(reading);
        }

        public async Task AddReadingsBatchAsync(List<SensorReading> readings)
        {
            await _repo.AddReadingsBatchAsync(readings);
        }

        public async Task<List<SensorReading>> GetReadingsBatchAsync(int startId, int batchSize)
        {
            return await _repo.GetReadingsBatchAsync(startId, batchSize);
        }
    }
}
