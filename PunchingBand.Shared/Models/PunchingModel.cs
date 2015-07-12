using System.ComponentModel;
using System.Security.Principal;
using Windows.ApplicationModel;
using Microsoft.Band;
using Microsoft.Band.Sensors;
using PunchingBand.Models.Enums;
using PunchingBand.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PunchingBand.Models
{
    public class PunchingModel : ModelBase
    {
        private readonly UserModel userModel;

        private IBandClient[] bandClients;
        private Dictionary<IBandSensor<IBandAccelerometerReading>, PunchDetector> punchDetectors;
        private Dictionary<IBandSensor<IBandAccelerometerReading>, DateTimeOffset> previousTimestamps;
        private Dictionary<IBandSensor<IBandContactReading>, BandTileModel> tiles; 

        private bool connected;
        private string status = "Connecting...";

        private int? heartRate;
        private bool heartRateLocked;
        private double? skinTemperature;
        private long stepCount;
        private double calorieCount;
        private DateTime? lastCaloriCountUpdate;
        private long? startStepCount;
        private bool worn;
        private FistSides fistSides;

        private readonly Action<Action> invokeOnUiThread;

        public event EventHandler<PunchEventArgs> PunchStarted = delegate { };
        public event EventHandler<PunchEventArgs> Punching = delegate { };
        public event EventHandler<PunchEventArgs> PunchEnded = delegate { };
        public event EventHandler StartFight = delegate { }; 

        public PunchingModel()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                throw new InvalidOperationException("Parameterless constructor can only be called by designer");
            }

            userModel = new UserModel();
            invokeOnUiThread = action => action();
        }

        public PunchingModel(UserModel userModel, Action<Action> invokeOnUiThread)
        {
            this.userModel = userModel;
            this.invokeOnUiThread = invokeOnUiThread;
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

        public FistSides FistSides
        {
            get { return fistSides; }
            set { Set("FistSides", ref fistSides, value); }
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

        public async Task Connect()
        {
#if MOCK_BAND
            Connected = true;
            Worn = true;
#else
            while (true)
            {
                if (!Connected)
                {
                    try
                    {
                        await ConnectCore();
                    }
                    catch (Exception ex)
                    {
                        Status = "Error connecting to Band!";
                        CleanupBandClients();
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
#endif
        }

        private async Task ConnectCore()
        {
            var bands = await BandClientManager.Instance.GetBandsAsync();

            if (bands.Length > 0)
            {
                bandClients = new IBandClient[bands.Length];
                punchDetectors = new Dictionary<IBandSensor<IBandAccelerometerReading>, PunchDetector>();
                previousTimestamps = new Dictionary<IBandSensor<IBandAccelerometerReading>, DateTimeOffset>();
                tiles = new Dictionary<IBandSensor<IBandContactReading>, BandTileModel>();

#if WINDOWS_APP
                var currentAppId = Guid.NewGuid();
#endif

                for (int i = 0; i < Math.Min(bands.Length, 2); i++)
                {
                    bandClients[i] = await BandClientManager.Instance.ConnectAsync(bands[i]);

                    await SetupBandTile(bandClients[i]);

                    // Only start fitness sensors on first band
                    if (i == 0)
                    {
                        await StartFitnessSensors(bandClients[i]);
                    }

                    // TODO: get fist side from band tile
                    await StartPunchDetection(bandClients[i], (FistSides)i);
                }

                Connected = true;
            }
            else
            {
                Status = "No Bands found!";
            }
        }

        private async Task StartFitnessSensors(IBandClient bandClient)
        {
            bandClient.SensorManager.Contact.ReadingChanged += ContactOnReadingChanged;
            bandClient.SensorManager.Pedometer.ReadingChanged += PedometerOnReadingChanged;
            bandClient.SensorManager.HeartRate.ReadingChanged += HeartRateOnReadingChanged;
            bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperatureOnReadingChanged;
            
            if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() != UserConsent.Granted)
            {
                await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }
            
            await bandClient.SensorManager.Contact.StartReadingsAsync();
            await bandClient.SensorManager.HeartRate.StartReadingsAsync();
            await bandClient.SensorManager.Pedometer.StartReadingsAsync();
            await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();
        }

        private async Task StartPunchDetection(IBandClient bandClient, FistSides fistSide)
        {
            punchDetectors[bandClient.SensorManager.Accelerometer] = new PunchDetector(fistSide);
            previousTimestamps[bandClient.SensorManager.Accelerometer] = DateTimeOffset.MinValue;

            bandClient.SensorManager.Accelerometer.ReadingChanged += AccelerometerOnReadingChanged;

            bandClient.SensorManager.Accelerometer.ReportingInterval =
                bandClient.SensorManager.Accelerometer.SupportedReportingIntervals.Min();

#if DEBUG
            punchDetectors[bandClient.SensorManager.Accelerometer].InitializeLogging();
#endif

            await bandClient.SensorManager.Accelerometer.StartReadingsAsync();
        }

        private async Task SetupBandTile(IBandClient bandClient)
        {
            tiles[bandClient.SensorManager.Contact] = new BandTileModel();

            tiles[bandClient.SensorManager.Contact].PropertyChanged += TileOnPropertyChanged;
            tiles[bandClient.SensorManager.Contact].FightButtonClick += (sender, args) => invokeOnUiThread(() => StartFight(this, EventArgs.Empty));

            await tiles[bandClient.SensorManager.Contact].Initialize(bandClient);
        }

        private void TileOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "FistSide")
            {
                invokeOnUiThread(() =>
                {
                    // TODO: support specifying both fists
                    var model = sender as BandTileModel;
                    FistSides = model.FistSide;
                });
            }
        }

        public void Disconnect()
        {
            if (!Connected || bandClients == null || bandClients[0] == null) return;

            CleanupBandClients();

            Connected = false;
            Worn = false;
        }

        private void CleanupBandClients()
        {
            if (bandClients != null)
            {
                foreach (var bandClient in bandClients.Where(b => b != null))
                {
                    bandClient.Dispose();
                }

                bandClients = null;
            }

            if (punchDetectors != null)
            {
                foreach (var punchDetector in punchDetectors.Values.Where(p => p != null))
                {
                    punchDetector.Dispose();
                }

                punchDetectors = null;
            }
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
                    Status = "Please wear your Band.";
                }
            });
        }

        private void AccelerometerOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> bandSensorReadingEventArgs)
        {
            var key = sender as IBandSensor<IBandAccelerometerReading>;

            if (!worn)
            {
                return;
            }

            var punchInfo = punchDetectors[key].GetPunchInfo(bandSensorReadingEventArgs.SensorReading);

            if (punchInfo.Status == PunchStatus.Finish)
            {
                invokeOnUiThread(() =>
                {
                    if (punchInfo.Strength.HasValue)
                    {
                        PunchEnded(this, new PunchEventArgs(punchInfo.FistSide, punchInfo.Strength.Value));
                    }
                });
            }
            else if (punchInfo.Status == PunchStatus.Start)
            {
                invokeOnUiThread(() =>
                {
                    PunchStarted(this, new PunchEventArgs(punchInfo.FistSide, punchInfo.Strength.Value));
                });
            }
            else if (punchInfo.Status == PunchStatus.InProgress)
            {
                invokeOnUiThread(() =>
                {
                    Punching(this, new PunchEventArgs(punchInfo.FistSide, punchInfo.Strength.Value));
                });
            }

            previousTimestamps[key] = bandSensorReadingEventArgs.SensorReading.Timestamp;
        }
    }
}
