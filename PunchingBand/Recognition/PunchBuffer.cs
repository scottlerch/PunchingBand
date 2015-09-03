using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Band.Portable.Sensors;
using PunchingBand.Band;

namespace PunchingBand.Recognition
{
    public class PunchBuffer : IEnumerable<GyroscopeAccelerometerReading>
    {
        // TODO: implement as circular buffer array for performance
        private readonly LinkedList<GyroscopeAccelerometerReading> buffer = new LinkedList<GyroscopeAccelerometerReading>();

        public PunchBuffer(int size)
        {
            Size = size;
            Clear();
        }

        public int Size { get; private set; }

        public void Add(GyroscopeAccelerometerReading reading)
        {
            if (buffer.Count >= Size)
            {
                buffer.RemoveFirst();
            }

            buffer.AddLast(reading);
        }

        public IEnumerator<GyroscopeAccelerometerReading> GetEnumerator()
        {
            return buffer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return buffer.GetEnumerator();
        }

        public void Clear()
        {
            buffer.Clear();

            for (int i = 0; i < Size; i++)
            {
                Add(GyroscopeAccelerometerReading.Empty);
            }
        }

        public void Reset(PunchBuffer punchBuffer, int numberOfSamplesBeforePunchStart)
        {
            Clear();

            foreach (var reading in punchBuffer.Skip(Size - numberOfSamplesBeforePunchStart))
            {
                Add(reading);
            }
        }
    }
}
