using Microsoft.Band.Portable.Sensors;
using PunchingBand.Band;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PunchingBand.Recognition
{
    public interface IPunchRecognizer
    {
        Task Initialize(FistSides fistSide);

        Task<PunchRecognition> Recognize(IEnumerable<GyroscopeAccelerometerReading> readings);
    }
}
