using Sensor_Api.Entities;

namespace Sensor_Api.Services
{
    public interface ISensorService
    {
        Task GenerateReadingAsync();
        Task<List<SensorReading>> GetReadingsBatchAsync(int startId, int batchSize);
        Task AddReadingsBatchAsync(List<SensorReading> readings);

    }
}
