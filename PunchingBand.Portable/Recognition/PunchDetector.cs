using Microsoft.Band.Sensors;
using PunchingBand.Recognition.Recognizers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PunchingBand.Recognition
{
    /// <summary>
    /// Detect punch from accelerometer data using simple hueristics.
    /// Once punch is detected a recognition will be attempted to determine the exact type of punch.
    /// </summary>
    public sealed class PunchDetector : IDisposable
    {
        /// <summary>
        /// Maximum acceleration in g's (1g = 9.81m/s^2).
        /// </summary>
        public const double MaximumAcceleration = 8.0;

        private readonly TimeSpan punchCoolOffInterval = TimeSpan.FromMilliseconds(100);
        private const double punchThreshold = 0.5;
        private const double punchResetThreshold = 0.0; 
        private const int punchVectorSize = 20; // ~320msec (20x16msec) max timeframe to detect punch
        private const int numberOfSamplesBeforePunchStart = 4; // 64msec of pre-punch data

        private double lastX;
        private double maxX = double.MinValue;

        private bool readyForPunch = true;

        private DateTime nextPunchTime = DateTime.MinValue;

        private PunchStatus lastPunchStatus = PunchStatus.Unknown;

        private bool punchStarted;

        // Sliding window of gryroscope/accelerometer readings
        private readonly PunchBuffer punchBuffer = new PunchBuffer(punchVectorSize);

        // Recording of gryroscope/acclerometer readings for current punch
        private readonly PunchBuffer currentPunchBuffer = new PunchBuffer(punchVectorSize);

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

        public FistSides FistSide { get { return fistSide; } }

        public async Task<PunchInfo> GetPunchInfo(IBandGyroscopeReading reading)
        {
            var status = PunchStatus.Unknown;
            double? punchStrength = null;
            var punchRecognition = PunchRecognition.Unknown;

            if (IsPunchDetected(reading, out punchStrength))
            {
                status = PunchStatus.Finish;

                punchRecognition = await DeterminePunchType(currentPunchBuffer).ConfigureAwait(false);

                punchLogger.LogPunchVector(currentPunchBuffer);
            }
            else
            {
                if (IsDetectingPunch(reading))
                {
                    // We know a punch is occuring but don't know the final strength
                    status = PunchStatus.Start;

                    currentPunchBuffer.Reset(punchBuffer, numberOfSamplesBeforePunchStart);
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
            currentPunchBuffer.Add(reading);

            lastPunchStatus = status;

            var punchInfo = new PunchInfo(fistSide, status, punchStrength, punchRecognition);

#if FULL_LOGGING
            punchLogger.LogPunchData(reading, punchInfo);
#endif

            return punchInfo;
        }

        private async Task<PunchRecognition> DeterminePunchType(IEnumerable<IBandGyroscopeReading> readings)
        {
            return await punchRecognizer.Recognize(readings);
        }

        private bool IsPunchDetected(IBandGyroscopeReading reading, out double? punchStrength)
        {
            punchStrength = null;
            bool punchDetected = false;

            if (readyForPunch)
            {
                if (reading.AccelerationX > punchThreshold)
                {
                    maxX = Math.Max(maxX, reading.AccelerationX);
                    punchStrength = ((maxX - punchThreshold) / (MaximumAcceleration - punchThreshold)) * MaximumAcceleration;

                    if (reading.AccelerationX >= MaximumAcceleration || reading.AccelerationX - lastX < 0)
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

        private bool IsDetectingPunch(IBandGyroscopeReading reading)
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
