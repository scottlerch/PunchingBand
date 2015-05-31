using System;

namespace PunchingBand.Infrastructure
{
    public struct Metric
    {
        private bool intialized;
        private int count;
        private double mean;
        private double min;
        private double max;
        private double last;

        public Metric(double value)
        {
            intialized = true;
            count = 1;
            min = value;
            max = value;
            mean = value;
            last = value;
        }

        public double Minimum { get { return min; } }

        public double Maximum { get { return max; } }

        public double Mean { get { return mean; } }

        public double Last { get { return last; } }

        public void Update(double value)
        {
            if (!intialized)
            {
                intialized = true;

                count = 1;
                min = value;
                max = value;
                last = value;
            }
            else
            {
                last = value;

                count++;

                min = Math.Min(value, min);
                max = Math.Max(value, max);

                mean = mean + ((value - mean) / count);
            }
        }
    }
}
