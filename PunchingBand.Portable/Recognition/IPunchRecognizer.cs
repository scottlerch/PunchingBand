using Microsoft.Band.Sensors;
using System.Collections.Generic;
using System.Threading.Tasks;
using PunchingBand.Models;

namespace PunchingBand.Recognition
{
    public interface IPunchRecognizer
    {
        Task Initialize(FistSides fistSide);

        Task<PunchRecognition> Recognize(IEnumerable<BandGyroscopeReading> readings);
    }
}
