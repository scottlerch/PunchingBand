using Microsoft.Band.Sensors;
using System.Collections;
using System.Collections.Generic;

namespace PunchingBand.Recognition
{
    public class PunchBuffer : IEnumerable<IBandGyroscopeReading>
    {
        // TODO: implement as circular buffer array for performance
        private readonly LinkedList<IBandGyroscopeReading> buffer = new LinkedList<IBandGyroscopeReading>();

        public PunchBuffer(int size)
        {
            Size = size;
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
    }
}
