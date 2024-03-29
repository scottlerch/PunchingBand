﻿using System.Collections.Generic;
using System.IO;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Band.Portable.Sensors;
using PunchingBand.Band;

namespace PunchingBand.Recognition
{
    public sealed class PunchLogger : IDisposable
    {
        private class PunchLogData
        {
            public PunchLogData(GyroscopeAccelerometerReading reading, PunchInfo punchInfo)
            {
                AccelerometerReading = reading;
                PunchInfo = punchInfo;
            }

            public GyroscopeAccelerometerReading AccelerometerReading { get; private set; }

            public PunchInfo PunchInfo { get; private set; }
        }

        private BlockingCollection<PunchLogData> logData;
        private Task logTask;

        private BlockingCollection<string> punchVectors;
        private Task punchVectorsTask;

        private FistSides fistSide;
        private Func<string, Task<Stream>> getWriteStream;

        public string TrainPunchType { get; set; }

        public PunchLogger(Func<string, Task<Stream>> getWriteStream)
        {
            this.getWriteStream = getWriteStream;
        }

        public void LogPunchData(GyroscopeAccelerometerReading reading, PunchInfo punchInfo)
        {
            if (logData != null)
            {
                logData.Add(new PunchLogData(reading, punchInfo));
            }
        }

        public void LogPunchVector(IEnumerable<GyroscopeAccelerometerReading> punchBuffer)
        {
            if (punchVectors != null)
            {
                punchVectors.Add(GetVectorCsv(punchBuffer));
            }
        }

        private string GetVectorCsv(IEnumerable<GyroscopeAccelerometerReading> punchBuffer)
        {
            var stringBuilder = new StringBuilder();
            foreach (var reading in punchBuffer)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(reading.AccelerationX);
                stringBuilder.Append(",");
                stringBuilder.Append(reading.AccelerationY);
                stringBuilder.Append(",");
                stringBuilder.Append(reading.AccelerationZ);
                stringBuilder.Append(",");
                stringBuilder.Append(reading.AngularVelocityX);
                stringBuilder.Append(",");
                stringBuilder.Append(reading.AngularVelocityY);
                stringBuilder.Append(",");
                stringBuilder.Append(reading.AngularVelocityZ);
            }
            return stringBuilder.ToString();
        }

        public Task Initialize(FistSides fistSide)
        {
            this.fistSide = fistSide;

            var originalLogData = logData;
            var originalLogTask = logTask;

            logData = new BlockingCollection<PunchLogData>();

            if (originalLogData != null)
            {
                originalLogData.CompleteAdding();
                originalLogTask.Wait();
                originalLogData.Dispose();
            }

            logTask = Task.Run(async () =>
            {
                var startTimestamp = DateTime.Now;

                using (var fileStream = await getWriteStream(string.Format("LogData/PunchData{0}.csv", fistSide)))
                {
                    var streamWriter = new StreamWriter(fileStream) { AutoFlush = true };

                    foreach (var data in logData.GetConsumingEnumerable())
                    {
                        streamWriter.WriteLine(
                            "{0},{1},{2},{3},{4},{5},{6},{7}",
                            (int)(data.AccelerometerReading.Timestamp - startTimestamp).TotalMilliseconds,
                            data.AccelerometerReading.AccelerationX,
                            data.AccelerometerReading.AccelerationY,
                            data.AccelerometerReading.AccelerationZ,
                            data.AccelerometerReading.AngularVelocityX,
                            data.AccelerometerReading.AngularVelocityY,
                            data.AccelerometerReading.AngularVelocityZ,
                            data.PunchInfo.Status);
                        streamWriter.Flush();
                        fileStream.Flush();
                    }
                }
            });

            var originalPunchVector = punchVectors;
            var originalPunchVectorTask = punchVectorsTask;

            punchVectors = new BlockingCollection<string>();

            if (originalPunchVector != null)
            {
                originalPunchVector.CompleteAdding();
                originalPunchVectorTask.Wait();
                originalPunchVector.Dispose();
            }

            punchVectorsTask = Task.Run(async () =>
            {
                using (var fileStream = await getWriteStream(string.Format("LogData/PunchVectors{0}.csv", fistSide)))
                {
                    var streamWriter = new StreamWriter(fileStream) { AutoFlush = true };

                    foreach (var vector in punchVectors.GetConsumingEnumerable())
                    {
                        streamWriter.WriteLine(TrainPunchType + "," + vector);
                        streamWriter.Flush();
                        fileStream.Flush();
                    }
                }
            });

            return Task.FromResult(0);
        }

        public void Dispose()
        {
            if (logData != null)
            {
                logData.CompleteAdding();
                logTask.Wait();
                logData.Dispose();
            }

            if (punchVectors != null)
            {
                punchVectors.CompleteAdding();
                punchVectorsTask.Wait();
                punchVectors.Dispose();
            }
        }
    }
}
