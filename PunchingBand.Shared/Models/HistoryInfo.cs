using System;
using PunchingBand.Infrastructure;

namespace PunchingBand.Models
{
    public class HistoryInfo
    {
        public DateTime Timestamp { get; set; }

        public TimeSpan Duration { get; set; }

        public int Score { get; set; }

        public FistSide FistSide { get; set; }

        public Metric CaloriesBurned { get; set; }

        public Metric PunchStrenth { get; set; }

        public Metric SkinTemperature { get; set; }

        public GameMode GameMode { get; set; }
    }
}
