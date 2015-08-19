//#define FULL_LOGGING
using Microsoft.Band.Portable.Sensors;
using PunchingBand.Band;
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

#if FULL_LOGGING
        private readonly PunchLogger punchLogger;
#endif

        public PunchDetector(Func<string, Task<Stream>> getReadStream, Func<string, Task<Stream>> getWriteStream)
        {
            punchRecognizer = new AccordNeuralNetworkRecognizer(getReadStream);
#if FULL_LOGGING
            punchLogger = new PunchLogger(getWriteStream);
#endif

            TrainPunchType = PunchType.Jab.ToString();
        }

        public double LastPunchStrength { get; private set; }

        public FistSides FistSide { get { return fistSide; } }
        
        public static FistSides PrimaryFistSide { get; set; }

        public async Task<PunchInfo> GetPunchInfo(GyroscopeAccelerometerReading reading)
        {
            var status = PunchStatus.Unknown;
            double? punchStrength = null;
            var punchRecognition = PunchRecognition.Unknown;

            if (IsPunchDetected(reading, out punchStrength))
            {
                status = PunchStatus.Finish;

                punchRecognition = await DeterminePunchType(currentPunchBuffer).ConfigureAwait(false);

                // HACK: ensure cross is always the opposite of the primary fist
                if (punchRecognition.PunchType == PunchType.Cross || punchRecognition.PunchType == PunchType.Jab)
                {
                    if (FistSide != PrimaryFistSide)
                    {
                        punchRecognition = new PunchRecognition(PunchType.Cross, punchRecognition.Confidence, punchRecognition.Delay);
                    }
                    else
                    {
                        punchRecognition = new PunchRecognition(PunchType.Jab, punchRecognition.Confidence, punchRecognition.Delay);
                    }
                }
              
#if FULL_LOGGING
                punchLogger.LogPunchVector(currentPunchBuffer);
#endif
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
            //punchLogger.LogPunchData(reading, punchInfo);
#endif

            return punchInfo;
        }

        private async Task<PunchRecognition> DeterminePunchType(IEnumerable<GyroscopeAccelerometerReading> readings)
        {
            return await punchRecognizer.Recognize(readings);
        }

        private bool IsPunchDetected(GyroscopeAccelerometerReading reading, out double? punchStrength)
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

        private bool IsDetectingPunch(GyroscopeAccelerometerReading reading)
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
#if FULL_LOGGING
            await punchLogger.Initialize(fistSide);
#endif
        }

        public string TrainPunchType
        {
#if FULL_LOGGING
            get { return punchLogger.TrainPunchType; }
            set { punchLogger.TrainPunchType = value; }
#else
            get; set;
#endif
        }

        public void Dispose()
        {
#if FULL_LOGGING
            punchLogger.Dispose();
#endif
        }
    }
}
