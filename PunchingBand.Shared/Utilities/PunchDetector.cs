using Microsoft.Band.Sensors;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using PunchingBand.Models.Enums;

namespace PunchingBand.Utilities
{
    internal sealed class PunchDetector : IDisposable
    {
        private class LogData
        {
            public IBandAccelerometerReading AccelerometerReading { get; set; }

            public PunchInfo PunchInfo { get; set; }
        }

        private readonly TimeSpan punchCoolOffInterval = TimeSpan.FromMilliseconds(100);
        private const double punchThreshold = 0.5;
        private const double punchResetThreshold = 0.0;
        private const double maximumAcceleration = 8.0;

        private double lastX;
        private double maxX = double.MinValue;

        private bool readyForPunch = true;

        private DateTime nextPunchTime = DateTime.MinValue;

        private bool punchStarted;

        private BlockingCollection<LogData> logData;
        private Task logTask;

        public double LastPunchStrength { get; private set; }

        private FistSides fistSide;

        public PunchDetector(FistSides fistSide)
        {
            this.fistSide = fistSide;
        }

        public PunchInfo GetPunchInfo(IBandAccelerometerReading reading)
        {
            var status = PunchStatus.Unknown;
            double? punchStrength;

            if (IsPunchDetected(reading, out punchStrength))
            {
                status = PunchStatus.Finish; 
            }
            else
            {
                if (IsDetectingPunch(reading))
                {
                    // We know a punch is occuring but don't know the final strength
                    status = PunchStatus.Start;
                }
                else if (punchStarted && punchStrength.HasValue)
                {
                    status = PunchStatus.InProgress;
                }
            }

            var punchInfo = new PunchInfo(fistSide, status, punchStrength);

            Log(reading, punchInfo);

            return punchInfo;
        }

        private bool IsPunchDetected(IBandAccelerometerReading reading, out double? punchStrength)
        {
            punchStrength = null;
            bool punchDetected = false;

            if (readyForPunch)
            {
                if (reading.AccelerationX > punchThreshold)
                {
                    maxX = Math.Max(maxX, reading.AccelerationX);
                    punchStrength = (maxX - punchThreshold) / (maximumAcceleration - punchThreshold);

                    if (reading.AccelerationX >= maximumAcceleration || reading.AccelerationX - lastX < 0)
                    {
                        LastPunchStrength = punchStrength.Value;

                        readyForPunch = false;
                        maxX = double.MinValue;

                        punchDetected = true;

                        nextPunchTime = DateTime.UtcNow + punchCoolOffInterval;
                    }
                }
            }
            else if (DateTime.UtcNow > nextPunchTime && reading.AccelerationX < punchResetThreshold)
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
        }

        private bool IsDetectingPunch(IBandAccelerometerReading reading)
        {
            if (!punchStarted && readyForPunch && reading.AccelerationX > punchThreshold)
            {
                punchStarted = true;
                return true;
            }

            return false;
        }

        private void Log(IBandAccelerometerReading reading, PunchInfo punchInfo)
        {
            if (logData != null)
            {
                logData.Add(new LogData
                {
                    AccelerometerReading = reading,
                    PunchInfo = punchInfo,
                });
            }
        }

        public void InitializeLogging()
        {
            logData = new BlockingCollection<LogData>();

            logTask = Task.Run(async () =>
            {
                var local = ApplicationData.Current.LocalFolder;
                var dataFolder = await local.CreateFolderAsync("LogData", CreationCollisionOption.OpenIfExists);
                var file = await dataFolder.CreateFileAsync(string.Format("PunchData{0}.csv", fistSide), CreationCollisionOption.ReplaceExisting);

                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    var streamWriter = new StreamWriter(fileStream) {AutoFlush = true};

                    foreach (var data in logData.GetConsumingEnumerable())
                    {
                        streamWriter.WriteLine(
                            "{0:HH:mm:ss.ffffff},{1},{2},{3},{4}",
                            data.AccelerometerReading.Timestamp,
                            data.AccelerometerReading.AccelerationX,
                            data.AccelerometerReading.AccelerationY,
                            data.AccelerometerReading.AccelerationZ,
                            data.PunchInfo.Status);
                    }
                }
            });
        }

        public void Dispose()
        {
            if (logData != null)
            {
                logData.CompleteAdding();
                logTask.Wait();
                logData.Dispose();
            }
        }
    }
}
