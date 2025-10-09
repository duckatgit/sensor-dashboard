using Sensor_Api.Entities;

namespace Sensor_Api.Repositories
{
    public interface ISensorRepository
    {
        Task AddReadingAsync(SensorReading reading);
        Task AddReadingsBatchAsync(List<SensorReading> readings);
        Task<List<SensorReading>> GetReadingsBatchAsync(int startId, int batchSize);

    }
}
