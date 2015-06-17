using System;
using PunchingBand.Models.Enums;

namespace PunchingBand.Models
{
    public class PunchEventArgs : EventArgs
    {
        public FistSides FistSide { get; private set; }

        public double Strength { get; private set; }

        public PunchEventArgs(FistSides fistSide, double strength)
        {
            FistSide = fistSide;
            Strength = strength;
        }
    }
}
