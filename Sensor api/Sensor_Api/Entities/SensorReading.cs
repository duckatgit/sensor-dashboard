namespace Sensor_Api.Entities
{
    public class SensorReading
    {
        public int Id { get; set; }
        public string SensorId { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
