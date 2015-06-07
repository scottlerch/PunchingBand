using Microsoft.Band.Sensors;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace PunchingBand.Utilities
{
    internal class PunchDetector
    {
        private class LogData
        {
            public IBandAccelerometerReading Reading { get; set; }

            public PunchInfo PunchInfo { get; set; }
        }

        private readonly TimeSpan fastestPunchInterval = TimeSpan.FromMilliseconds(100);
        private const double punchThreshold = 0.5;
        private const double punchResetThreshold = 0.1;
        private const double maximumAcceleration = 8.0;

        private double lastX;
        private double maxX = double.MinValue;

        private bool readyForPunch = true;

        private DateTime lastPunchTime = DateTime.MinValue;

        private bool punchStarted;

        private BlockingCollection<LogData> logData;

        public double LastPunchStrength { get; private set; }

        public PunchInfo GetPunchInfo(IBandAccelerometerReading reading)
        {
            var status = PunchStatus.Unknown;
            double? punchStrength;

            if (IsPunchDetected(reading, out punchStrength))
            {
                status = PunchStatus.Detected; 
            }
            else
            {
                if (IsDetectingPunch(reading))
                {
                    // We know a punch is occuring but don't know the final strength
                    status = PunchStatus.Detecting;
                }
            }

            var punchInfo = new PunchInfo(status, punchStrength);

            Log(reading, punchInfo);

            return punchInfo;
        }

        private bool IsPunchDetected(IBandAccelerometerReading reading, out double? punchStrength)
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

        private bool IsDetectingPunch(IBandAccelerometerReading reading)
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

        private void Log(IBandAccelerometerReading reading, PunchInfo punchInfo)
        {
            if (logData != null)
            {
                logData.Add(new LogData
                {
                    Reading = reading,
                    PunchInfo = punchInfo,
                });
            }
        }

        public void InitializeLogging()
        {
            logData = new BlockingCollection<LogData>();

            Task.Run(async () =>
            {
                var local = ApplicationData.Current.LocalFolder;
                var dataFolder = await local.CreateFolderAsync("LogData", CreationCollisionOption.OpenIfExists);
                var file = await dataFolder.CreateFileAsync("PunchData.csv", CreationCollisionOption.ReplaceExisting);

                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var streamWriter = new StreamWriter(fileStream) {AutoFlush = true};

                    foreach (var data in logData.GetConsumingEnumerable())
                    {
                        streamWriter.WriteLine(
                            "{0:yyyy-MM-dd HH:mm:ss.ffffff},{1},{2}",
                            data.Reading.Timestamp,
                            data.Reading.AccelerationX,
                            data.PunchInfo.Status);
                    }
                }
            });
        }
    }
}
