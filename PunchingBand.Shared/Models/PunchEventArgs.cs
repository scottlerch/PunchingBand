using System;

namespace PunchingBand.Models
{
    public class PunchEventArgs : EventArgs
    {
        public FistSide FistSide { get; private set; }

        public double Strength { get; private set; }

        public PunchEventArgs(FistSide fistSide, double strength)
        {
            FistSide = fistSide;
            Strength = strength;
        }
    }
}
