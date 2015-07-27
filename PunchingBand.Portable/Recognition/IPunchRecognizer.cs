using Microsoft.Band.Sensors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PunchingBand.Recognition
{
    public interface IPunchRecognizer
    {
        Task Initialize(FistSides fistSide);

        Task<PunchRecognition> Recognize(IEnumerable<IBandGyroscopeReading> readings);
    }
}
