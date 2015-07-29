using System.Linq;
using Microsoft.Band.Sensors;
using System.Collections;
using System.Collections.Generic;
using PunchingBand.Models;

namespace PunchingBand.Recognition
{
    public class PunchBuffer : IEnumerable<BandGyroscopeReading>
    {
        // TODO: implement as circular buffer array for performance
        private readonly LinkedList<BandGyroscopeReading> buffer = new LinkedList<BandGyroscopeReading>();

        public PunchBuffer(int size)
        {
            Size = size;
            Clear();
        }

        public int Size { get; private set; }

        public void Add(BandGyroscopeReading reading)
        {
            if (buffer.Count >= Size)
            {
                buffer.RemoveFirst();
            }

            buffer.AddLast(reading);
        }

        public IEnumerator<BandGyroscopeReading> GetEnumerator()
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
