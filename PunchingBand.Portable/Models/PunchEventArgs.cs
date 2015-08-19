using System;
using PunchingBand.Recognition;

namespace PunchingBand.Models
{
    public class PunchEventArgs : EventArgs
    {
        public FistSides FistSide { get; private set; }

        public double? Strength { get; private set; }

        public PunchRecognition PunchRecognition { get; private set; }

        public PunchEventArgs(FistSides fistSide, double? strength, PunchRecognition punchRecognition)
        {
            FistSide = fistSide;
            Strength = strength;
            PunchRecognition = punchRecognition;
        }
    }
}
