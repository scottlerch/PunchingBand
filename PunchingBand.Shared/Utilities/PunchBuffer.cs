using Microsoft.Band.Sensors;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PunchingBand.Utilities
{
    internal class PunchBuffer : IEnumerable<IBandAccelerometerReading>
    {
        private LinkedList<IBandAccelerometerReading> buffer = new LinkedList<IBandAccelerometerReading>();

        public PunchBuffer(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        public void Add(IBandAccelerometerReading reading)
        {
            if (buffer.Count >= Size)
            {
                buffer.RemoveFirst();
            }

            buffer.AddLast(reading);
        }

        public IEnumerator<IBandAccelerometerReading> GetEnumerator()
        {
            return buffer.GetEnumerator();
        }

        public string GetVector()
        {
            var stringBuilder = new StringBuilder();
            foreach (var reading in buffer)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(reading.AccelerationX);
                stringBuilder.Append(",");
                stringBuilder.Append(reading.AccelerationY);
                stringBuilder.Append(",");
                stringBuilder.Append(reading.AccelerationZ);
            }
            return stringBuilder.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return buffer.GetEnumerator();
        }
    }
}
