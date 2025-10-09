using System.Collections;

namespace Sensor_Api.Utils
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _head;
        private int _count;

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero.");

            _buffer = new T[capacity];
            _head = 0;
            _count = 0;
        }

        public int Count => _count;
        public int Capacity => _buffer.Length;

        public void Add(T item)
        {
            _buffer[_head] = item;
            _head = (_head + 1) % Capacity;
            if (_count < Capacity)
                _count++;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _buffer[(_head - _count + i + Capacity) % Capacity];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _head = 0;
            _count = 0;
        }
    }
}
