using Accord;
using Microsoft.Band.Sensors;
using PunchingBand.Recognition.Recognizers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PunchingBand.Recognition
{
    /// <summary>
    /// Detect punch from accelerometer data using simple hueristics.
    /// Once punch is detected a recognition will be attempted to determine the exact type of punch.
    /// </summary>
    public sealed class PunchDetector : IDisposable
    {
        private readonly TimeSpan punchCoolOffInterval = TimeSpan.FromMilliseconds(100);
        private const double punchThreshold = 0.5;
        private const double punchResetThreshold = 0.0;
        private const double maximumAcceleration = 8.0;
        private const int punchVectorSize = 10; // ~160msec (10x16msec) timeframe to detect punch

        private double lastX;
        private double maxX = double.MinValue;

        private bool readyForPunch = true;

        private DateTime nextPunchTime = DateTime.MinValue;

        private PunchStatus lastPunchStatus = PunchStatus.Unknown;

        private bool punchStarted;

        private readonly PunchBuffer punchBuffer = new PunchBuffer(punchVectorSize);
        private int? punchBufferWindowCount;

        private FistSides fistSide = FistSides.Unknown;

        private readonly IPunchRecognizer punchRecognizer;
        private readonly PunchLogger punchLogger;

        public PunchDetector(Func<string, Task<Stream>> getReadStream, Func<string, Task<Stream>> getWriteStream)
        {
            punchRecognizer = new AccordNeuralNetworkRecognizer(getReadStream);
            punchLogger = new PunchLogger(getWriteStream);

            TrainPunchType = PunchType.Jab.ToString();
        }

        public double LastPunchStrength { get; private set; }

        public async Task<PunchInfo> GetPunchInfo(IBandAccelerometerReading reading)
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
                    // Use previous previous values in recognizing punch type
                    punchBufferWindowCount = 2;
                    // We know a punch is occuring but don't know the final strength
                    status = PunchStatus.Start;
                }
                else if (punchStarted && punchStrength.HasValue)
                {
                    status = PunchStatus.InProgress;
                }
                else if (!readyForPunch)
                {
                    status = PunchStatus.Resetting;
                }
                else if (lastPunchStatus == PunchStatus.Resetting)
                {
                    status = PunchStatus.Reset;
                }
            }

            punchBuffer.Add(reading);

            if (punchBufferWindowCount.HasValue)
            {
                punchBufferWindowCount++;
            }

            var bufferFull = punchBufferWindowCount >= punchBuffer.Size;

            if (bufferFull)
            {
                punchLogger.LogPunchVector(punchBuffer);
                punchBufferWindowCount = null;
            }

            var punchInfo = new PunchInfo(fistSide, status, punchStrength, PunchRecognition.Unknown);

            punchLogger.LogPunchData(reading, punchInfo);

            lastPunchStatus = status;

            if (bufferFull)
            {
                var punchRecognition = await DeterminePunchType(punchBuffer.ToList()).ConfigureAwait(false);
                return new PunchInfo(fistSide, status, punchStrength, punchRecognition);
            }
            else
            {
                return punchInfo;
            }
        }

        private async Task<PunchRecognition> DeterminePunchType(IEnumerable<IBandAccelerometerReading> readings)
        {
            return await punchRecognizer.Recognize(readings);
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

        public async Task Initialize(FistSides fistSide)
        {
            this.fistSide = fistSide;

            await punchRecognizer.Initialize(fistSide);
            await punchLogger.Initialize(fistSide);
        }

        public string TrainPunchType
        {
            get { return punchLogger.TrainPunchType; }
            set { punchLogger.TrainPunchType = value; }
        }

        public void Dispose()
        {
            punchLogger.Dispose();
        }
    }
}
