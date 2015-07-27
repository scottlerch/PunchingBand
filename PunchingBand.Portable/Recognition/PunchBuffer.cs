using System.Linq;
using Microsoft.Band.Sensors;
using System.Collections;
using System.Collections.Generic;

namespace PunchingBand.Recognition
{
    public class PunchBuffer : IEnumerable<IBandGyroscopeReading>
    {
        private class BandGyroscopeReading : IBandGyroscopeReading
        {
            public static readonly IBandGyroscopeReading Empty = new BandGyroscopeReading();

            public double AngularVelocityX { get; set; }

            public double AngularVelocityY { get; set; }

            public double AngularVelocityZ { get; set; }

            public double AccelerationX { get; set; }

            public double AccelerationY { get; set; }

            public double AccelerationZ { get; set; }

            public System.DateTimeOffset Timestamp { get; set; }
        }

        // TODO: implement as circular buffer array for performance
        private readonly LinkedList<IBandGyroscopeReading> buffer = new LinkedList<IBandGyroscopeReading>();

        public PunchBuffer(int size)
        {
            Size = size;
            Clear();
        }

        public int Size { get; private set; }

        public void Add(IBandGyroscopeReading reading)
        {
            if (buffer.Count >= Size)
            {
                buffer.RemoveFirst();
            }

            buffer.AddLast(reading);
        }

        public IEnumerator<IBandGyroscopeReading> GetEnumerator()
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
                Add(BandGyroscopeReading.Empty);
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
