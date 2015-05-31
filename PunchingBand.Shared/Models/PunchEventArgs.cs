using System;

namespace PunchingBand.Models
{
    public class PunchEventArgs : EventArgs
    {
        public double Strength { get; private set; }

        public PunchEventArgs(double strength)
        {
            Strength = strength;
        }
    }
}
