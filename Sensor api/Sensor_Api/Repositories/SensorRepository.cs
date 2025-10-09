using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sensor_Api.Data;
using Sensor_Api.Entities;
using System.Collections.Concurrent;

namespace Sensor_Api.Repositories
{
    public class SensorRepository : ISensorRepository
    {
        private readonly SensorDbContext _context;
        private readonly ConcurrentQueue<SensorReading> _queue = new();

        public SensorRepository(SensorDbContext context)
        {
            _context = context;
        }

        public Task AddReadingAsync(SensorReading reading)
        {
            _queue.Enqueue(reading);
            return Task.CompletedTask;
        }

        //public Task AddReadingsBatchAsync(List<SensorReading> readings)
        //{
        //    foreach (var r in readings)
        //        _queue.Enqueue(r);
        //    return Task.CompletedTask;
        //}
        public async Task AddReadingsBatchAsync(List<SensorReading> readings)
        {
            _context.SensorReadings.AddRange(readings);
            await _context.SaveChangesAsync();
        }


        public async Task<List<SensorReading>> GetReadingsBatchAsync(int startId, int batchSize)
        {
            var startIdParam = new SqlParameter("@StartId", startId);
            var batchSizeParam = new SqlParameter("@BatchSize", batchSize);

            return await _context.SensorReadings
                .FromSqlRaw("EXEC usp_GetSensorReadingsBatch @StartId, @BatchSize", startIdParam, batchSizeParam)
                .ToListAsync();
        }
    }
}
