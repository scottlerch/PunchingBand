using Microsoft.Band.Sensors;
using System.Collections;
using System.Collections.Generic;

namespace PunchingBand.Recognition
{
    public class PunchBuffer : IEnumerable<IBandAccelerometerReading>
    {
        // TODO: implement as circular buffer array for performance
        private readonly LinkedList<IBandAccelerometerReading> buffer = new LinkedList<IBandAccelerometerReading>();

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return buffer.GetEnumerator();
        }
    }
}
