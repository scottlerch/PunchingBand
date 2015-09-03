using System;

namespace PunchingBand.Recognition
{
    /// <summary>
    /// Punch recognition result.
    /// </summary>
    public class PunchRecognition
    {
        public static PunchRecognition Unknown = new PunchRecognition(PunchType.Unknown, 0, TimeSpan.Zero);

        public PunchRecognition(PunchType punchType, double confidence, TimeSpan delay)
        {
            Confidence = confidence;
            PunchType = punchType;
            Delay = delay;
        }

        public double Confidence { get; private set; }

        public PunchType PunchType { get; private set; }

        public TimeSpan Delay { get; private set; }
    }
}
