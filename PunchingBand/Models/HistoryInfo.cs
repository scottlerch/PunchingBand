using System;

namespace PunchingBand.Models
{
    public class HistoryInfo
    {
        public TimeSpan Duration { get; set; }

        public int Score { get; set; }

        public FistSide Side { get; set; }

        public int CaloriesBurned { get; set; }

        public GameMode GameMode { get; set; }
    }
}
