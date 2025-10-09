using Microsoft.EntityFrameworkCore;
using Sensor_Api.Entities;

namespace Sensor_Api.Data
{
    public class SensorDbContext : DbContext
    {
        public SensorDbContext(DbContextOptions<SensorDbContext> options)
            : base(options) { }

        public DbSet<SensorReading> SensorReadings { get; set; }
    }
}
