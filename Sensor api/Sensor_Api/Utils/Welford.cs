namespace Sensor_Api.Utils
{
    public class Welford
    {
        private int _count;
        private double _mean;
        private double _m2;

        public void AddSample(double value)
        {
            _count++;
            var delta = value - _mean;
            _mean += delta / _count;
            var delta2 = value - _mean;
            _m2 += delta * delta2;
        }

        public double Mean => _mean;
        public double Variance => _count > 1 ? _m2 / (_count - 1) : 0.0;
        public double StandardDeviation => Math.Sqrt(Variance);
        public int Count => _count;
    }
}
