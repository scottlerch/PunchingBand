﻿using Microsoft.Band.Sensors;
using System;

namespace PunchingBand
{
    internal class PunchDetector
    {
        private readonly TimeSpan fastestPunchInterval = TimeSpan.FromMilliseconds(200);
        private const double punchThreshold = 0.5;
        private const double punchResetThreshold = 0.1;
        private const double maximumAcceleration = 8.0;

        private double lastX;
        private double maxX = double.MinValue;

        private bool readyForPunch = true;

        private DateTime lastPunchTime = DateTime.MinValue;

        private bool punchStarted;

        public double LastPunchStrength { get; private set; }

        public bool IsPunchDetected(IBandAccelerometerReading reading, out double? punchStrength)
        {
            punchStrength = null;
            bool punchDetected = false;

            if (readyForPunch)
            {
                if (reading.AccelerationX > punchThreshold && (DateTime.UtcNow - lastPunchTime) > fastestPunchInterval)
                {
                    maxX = Math.Max(maxX, reading.AccelerationX);

                    if (reading.AccelerationX >= maximumAcceleration || reading.AccelerationX - lastX < 0)
                    {
                        LastPunchStrength = (maxX - punchThreshold) / (maximumAcceleration - punchThreshold);
                        punchStrength = LastPunchStrength;

                        readyForPunch = false;
                        lastPunchTime = DateTime.UtcNow;
                        maxX = double.MinValue;

                        punchDetected = true;
                    }
                }
            }
            else if (reading.AccelerationX < punchResetThreshold)
            {
                punchStarted = false;
                readyForPunch = true;
                maxX = double.MinValue;
            }

            lastX = reading.AccelerationX;

            return punchDetected;
        }

        public void Reset()
        {
            lastX = 0;
            maxX = 0;

            readyForPunch = true;

            lastPunchTime = DateTime.MinValue;
        }

        internal bool IsDetectingPunch(IBandAccelerometerReading reading)
        {
            if (!punchStarted && readyForPunch)
            {
                if (reading.AccelerationX > punchThreshold && (DateTime.UtcNow - lastPunchTime) > fastestPunchInterval)
                {
                    punchStarted = true;
                    return true;
                }
            }

            return false;
        }
    }
}