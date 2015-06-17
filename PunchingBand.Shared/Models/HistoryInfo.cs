﻿using System;
using PunchingBand.Infrastructure;
using PunchingBand.Models.Enums;

namespace PunchingBand.Models
{
    public class HistoryInfo
    {
        public DateTime Timestamp { get; set; }

        public TimeSpan Duration { get; set; }

        public int Score { get; set; }

        public FistSides FistSide { get; set; }

        public Metric CaloriesBurned { get; set; }

        public Metric PunchStrenth { get; set; }

        public int PunchCount { get; set; }

        public Metric SkinTemperature { get; set; }

        public GameMode GameMode { get; set; }
    }
}
