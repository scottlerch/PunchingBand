using System;
using PunchingBand.Models.Enums;

namespace PunchingBand.Models
{
    public class PunchEventArgs : EventArgs
    {
        public FistSides FistSide { get; private set; }

        public double Strength { get; private set; }

        public PunchType PunchType { get; private set; }

        public PunchEventArgs(FistSides fistSide, double strength, PunchType punchType = PunchType.Unknown)
        {
            FistSide = fistSide;
            Strength = strength;
            PunchType = punchType;
        }
    }
}
