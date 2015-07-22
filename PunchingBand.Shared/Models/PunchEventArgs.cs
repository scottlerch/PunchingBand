using System;
using PunchingBand.Models.Enums;

namespace PunchingBand.Models
{
    public class PunchEventArgs : EventArgs
    {
        public FistSides FistSide { get; private set; }

        public double Strength { get; private set; }

        public PunchType PunchType { get; private set; }

        public int RecognitionDelay { get; private set; }

        public PunchEventArgs(FistSides fistSide, double strength, PunchType punchType = PunchType.Unknown, int recognitionDelay = 0)
        {
            FistSide = fistSide;
            Strength = strength;
            PunchType = punchType;
            RecognitionDelay = recognitionDelay;
        }
    }
}
