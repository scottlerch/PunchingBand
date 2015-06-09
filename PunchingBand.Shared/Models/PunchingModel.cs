using Microsoft.Band;
using Microsoft.Band.Sensors;
using PunchingBand.Utilities;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PunchingBand.Models
{
    public class PunchingModel : ModelBase
    {
        private readonly UserModel userModel;
        private IBandClient bandClient;
        private bool connected;
        private string status = "Connecting...";

        private readonly PunchDetector punchDetector = new PunchDetector();

        private int? heartRate;
        private bool heartRateLocked;
        private double? skinTemperature;
        private long stepCount;
        private double calorieCount;
        private DateTime? lastCaloriCountUpdate;
        private long? startStepCount;
        private bool worn;

        private readonly Action<Action> invokeOnUiThread;

        public event EventHandler<PunchEventArgs> PunchStarted = delegate { };
        public event EventHandler<PunchEventArgs> Punching = delegate { };
        public event EventHandler<PunchEventArgs> PunchEnded = delegate { };

        public PunchingModel()
        {
            userModel = new UserModel();
            invokeOnUiThread = action => action();
        }

        public PunchingModel(UserModel userModel, Action<Action> invokeOnUiThread)
        {
            this.userModel = userModel;
            this.invokeOnUiThread = invokeOnUiThread;

#if DEBUG
            punchDetector.InitializeLogging();
#endif
        }

        public int? HeartRate
        {
            get { return heartRate; }
            set { Set("HeartRate", ref heartRate, value); }
        }

        public bool HeartRateLocked
        {
            get { return heartRateLocked; }
            set { Set("HeartRateLocked", ref heartRateLocked, value); }
        }

        public double CalorieCount
        {
            get { return calorieCount; }
            private set { Set("CalorieCount", ref calorieCount, value); }
        }

        public double? SkinTemperature
        {
            get { return skinTemperature; }
            set { Set("SkinTemperature", ref skinTemperature, value); }
        }

        public long StepCount
        {
            get { return stepCount; }
            set { Set("StepCount", ref stepCount, value); }
        }

        public bool Worn
        {
            get { return worn; }
            set { Set("Worn", ref worn, value); }
        }

        public bool Connected
        {
            get { return connected; }
            set { Set("Connected", ref connected, value); }
        }

        public string Status
        {
            get { return status; }
            set { Set("Status", ref status, value); }
        }

        private bool connecting;
        private readonly object syncRoot = new object();

        public async void Connect()
        {
#if MOCK_BAND
            Connected = true;
            Worn = true;
#else
            lock (syncRoot)
            {
                if (connecting)
                {
                    return;
                }

                connecting = true;
            }

            while (!Connected)
            {
                try
                {
                    var bands = await BandClientManager.Instance.GetBandsAsync();

                    if (bands.Length > 0)
                    {
                        bandClient = await BandClientManager.Instance.ConnectAsync(bands[0]);

                        Type.GetType("Microsoft.Band.BandClient, Microsoft.Band")
                            .GetRuntimeFields()
                            .First(field => field.Name == "currentAppId")
                            .SetValue(bandClient, Guid.NewGuid());

                        bandClient.SensorManager.Accelerometer.ReadingChanged += AccelerometerOnReadingChanged;
                        bandClient.SensorManager.Contact.ReadingChanged += ContactOnReadingChanged;
                        bandClient.SensorManager.Pedometer.ReadingChanged += PedometerOnReadingChanged;
                        bandClient.SensorManager.HeartRate.ReadingChanged += HeartRateOnReadingChanged;
                        bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperatureOnReadingChanged;

                        bandClient.SensorManager.Accelerometer.ReportingInterval =
                            bandClient.SensorManager.Accelerometer.SupportedReportingIntervals.Min();

                        if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() != UserConsent.Granted)
                        {
                            await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
                        }

                        await bandClient.SensorManager.Accelerometer.StartReadingsAsync();
                        await bandClient.SensorManager.Contact.StartReadingsAsync();
                        await bandClient.SensorManager.HeartRate.StartReadingsAsync();
                        await bandClient.SensorManager.Pedometer.StartReadingsAsync();
                        await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();

                        Connected = true;

                        if (!Worn)
                        {
                            Status = "Band connected!  Please wear...";
                        }
                    }
                    else
                    {
                        Status = "No Band found!";
                    }
                }
                catch (Exception)
                {
                    Status = "Error connecting to Band!";

                    if (bandClient != null)
                    {
                        bandClient.Dispose();
                        bandClient = null;
                    }
                }

                if (!Connected)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }

            connecting = false;
#endif
        }

        public void Disconnect()
        {
            if (!Connected || bandClient == null) return;

            bandClient.Dispose();
            bandClient = null;

            punchDetector.Reset();

            Connected = false;
            Worn = false;
        }

        private void SkinTemperatureOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandSkinTemperatureReading> bandSensorReadingEventArgs)
        {
            invokeOnUiThread(() => SkinTemperature = (bandSensorReadingEventArgs.SensorReading.Temperature * 1.8) + 32.0);
        }

        private void HeartRateOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> bandSensorReadingEventArgs)
        {
            invokeOnUiThread(() =>
            {
                HeartRate = bandSensorReadingEventArgs.SensorReading.HeartRate;
                HeartRateLocked = bandSensorReadingEventArgs.SensorReading.Quality == HeartRateQuality.Locked;

                if (lastCaloriCountUpdate.HasValue && heartRate.HasValue)
                {
                    CalorieCount = CalorieCalculator.GetCalories(
                        userModel.Gender,
                        heartRate.Value,
                        userModel.Weight,
                        userModel.Age,
                        DateTime.UtcNow - lastCaloriCountUpdate.Value);
                }
                else
                {
                    lastCaloriCountUpdate = DateTime.UtcNow;
                }
            });
        }

        private void PedometerOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandPedometerReading> bandSensorReadingEventArgs)
        {
            if (startStepCount == null)
            {
                startStepCount = bandSensorReadingEventArgs.SensorReading.TotalSteps;
            }
            else
            {
                invokeOnUiThread(() => StepCount = bandSensorReadingEventArgs.SensorReading.TotalSteps - startStepCount.Value);
            }
        }

        private void ContactOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandContactReading> bandSensorReadingEventArgs)
        {
            invokeOnUiThread(() =>
            {
                if (bandSensorReadingEventArgs.SensorReading.State == BandContactState.Worn)
                {
                    Worn = true;
                    Status = string.Empty;
                }
                else
                {
                    Worn = false;
                    Status = "Band is not being worn!";
                }
            });
        }

        private DateTimeOffset previousTimestamp = DateTimeOffset.MinValue;

        private void AccelerometerOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> bandSensorReadingEventArgs)
        {
            // HACK: due to bug in API throw out duplicate timestamps which have all zero values anyway
            if (!worn || previousTimestamp == bandSensorReadingEventArgs.SensorReading.Timestamp)
            {
                return;
            }

            var punchInfo = punchDetector.GetPunchInfo(bandSensorReadingEventArgs.SensorReading);

            if (punchInfo.Status == PunchStatus.Finish)
            {
                invokeOnUiThread(() =>
                {
                    if (punchInfo.Strength.HasValue)
                    {
                        PunchEnded(this, new PunchEventArgs(punchInfo.Strength.Value));
                    }
                });
            }
            else if (punchInfo.Status == PunchStatus.Start)
            {
                invokeOnUiThread(() =>
                {
                    PunchStarted(this, new PunchEventArgs(punchInfo.Strength.Value));
                });
            }
            else if (punchInfo.Status == PunchStatus.InProgress)
            {
                invokeOnUiThread(() =>
                {
                    Punching(this, new PunchEventArgs(punchInfo.Strength.Value));
                });
            }

            previousTimestamp = bandSensorReadingEventArgs.SensorReading.Timestamp;
        }
    }
}
