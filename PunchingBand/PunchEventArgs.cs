using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PunchingBand
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
