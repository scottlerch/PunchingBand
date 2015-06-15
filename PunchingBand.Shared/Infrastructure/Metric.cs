using Newtonsoft.Json;
using System;

namespace PunchingBand.Infrastructure
{
    public class Metric
    {
        public Metric()
        {
        }

        public Metric(double value)
        {
            Update(value);
        }

        [JsonProperty]
        public double Minimum { get; private set; }

        [JsonProperty]
        public double Maximum { get; private set; }

        [JsonProperty]
        public double Mean { get; private set; }

        [JsonProperty]
        public double Last { get; private set; }

        [JsonProperty]
        public int Count { get; private set; }

        public void Update(double value)
        {
            Last = value;
            Count++;

            if (Count == 1)
            {
                Minimum = value;
                Maximum = value;
                Mean = value;
            }
            else
            {
                Minimum = Math.Min(value, Minimum);
                Maximum = Math.Max(value, Maximum);

                Mean = Mean + ((value - Mean) / Count);
            }
        }
    }
}
