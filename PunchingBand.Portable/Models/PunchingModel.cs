using System.Diagnostics;
using Microsoft.Band.Portable;
using Microsoft.Band.Portable.Sensors;
using PunchingBand.Recognition;
using PunchingBand.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace PunchingBand.Models
{
    public class PunchingModel : ModelBase
    {
        private readonly UserModel userModel;

        private readonly List<PunchBand> punchBands = new List<PunchBand>();

        private bool connected;
        private string status = "Connecting...";

        private int? heartRate;
        private bool heartRateLocked;
        private double? skinTemperature;
        private long stepCount;
        private double calorieCount;
        private DateTime? lastCalorieCountUpdate;
        private long? startStepCount;
        private bool worn;
        private FistSides fistSides = FistSides.Unknown;
        private PunchBand fitnessSensorsPunchBand = null;

        private readonly Action<Action> invokeOnUiThread;
        private readonly Func<string, Task<Stream>> getReadStream;
        private readonly Func<string, Task<Stream>> getWriteStream;

        public event EventHandler<PunchEventArgs> PunchStarted = delegate { };
        public event EventHandler<PunchEventArgs> Punching = delegate { };
        public event EventHandler<PunchEventArgs> PunchEnded = delegate { };
        public event EventHandler<PunchEventArgs> PunchRecognized = delegate { };
        public event EventHandler StartFight = delegate { }; 

        public PunchingModel()
        {
            if (!PortableDesignMode.DesignModeEnabled)
            {
                throw new InvalidOperationException("Parameterless constructor can only be called by designer");
            }

            userModel = new UserModel();
            invokeOnUiThread = action => action();
        }

        public PunchingModel(UserModel userModel, Action<Action> invokeOnUiThread, Func<string, Task<Stream>> getReadStream, Func<string, Task<Stream>> getWriteStream)
        {
            this.userModel = userModel;
            this.invokeOnUiThread = invokeOnUiThread;

            this.getReadStream = getReadStream;
            this.getWriteStream = getWriteStream;
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
            set { Set("Worn", ref worn, value); RaisePropertyChanged("Ready"); }
        }

        public bool Ready
        {
            get { return worn && fistSides != FistSides.Unknown; }
        }

        public FistSides FistSides
        {
            get { return fistSides; }
            private set { Set("FistSides", ref fistSides, value); RaisePropertyChanged("Ready"); }
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
            FistSides = FistSides.Right;
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
                        Debug.WriteLine("Error connecting to Band: {0}", ex);
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
            var sw = Stopwatch.StartNew();
            var bands = (await BandClientManager.Instance.GetPairedBandsAsync()).ToArray();
            sw.Stop();
            Debug.WriteLine("Get Bands: {0}", sw.Elapsed);

            if (bands.Length > 0)
            {
                var connectExceptions = new List<Exception>();

                for (int i = 0; i < Math.Min(bands.Length, 2); i++)
                {
                    BandClient bandClient;

                    try
                    {
                        sw.Restart();
                        bandClient = await BandClientManager.Instance.ConnectAsync(bands[i]);
                        Debug.WriteLine("Connect Band: {0}", sw.Elapsed);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error connecting to Band: {0}", ex);
                        connectExceptions.Add(ex);
                        continue;
                    }

                    var punchBand = new PunchBand(bandClient, getReadStream, getWriteStream);

                    punchBands.Add(punchBand);

                    punchBand.FistSideSelected += PunchBandOnFistSideSelected;
                    punchBand.PunchInfoChanged += PunchBandOnPunchInfoChanged;
                    punchBand.WornChanged += PunchBandOnWornChanged;
                    punchBand.StartFight += PunchBandOnStartFight;

                    sw.Restart();
                    await punchBand.Initialize();
                    Debug.WriteLine("Initialize Band: {0}", sw.Elapsed);
                }

                // If all connections failed rethrow exception, otherwise 1 band is good enough
                if (connectExceptions.Count == bands.Length)
                {
                    throw new AggregateException(connectExceptions);
                }

                Connected = true;
            }
            else
            {
                Status = "No Bands found!";
            }
        }

        private async void PunchBandOnWornChanged(object sender, EventArgs eventArgs)
        {
            var punchBand = sender as PunchBand;

            Debug.WriteLine("Band Worn Changed - Side:{0}  Worn:{1}  TileSide:{2}", punchBand.FistSide, punchBand.Worn, punchBand.BandTile.FistSide);

            invokeOnUiThread(async () =>
            {
                // Consider worn if either wrist has band worn
                Worn = punchBands.Any(b => b.Worn);

                if (!Worn)
                {
                    FistSides = FistSides.Unknown;
                    foreach (var band in punchBands)
                    {
                        await band.ForceFistSide(FistSides.Unknown);
                    }
                }

                Status = Worn ? string.Empty : "Please wear your Band.";

                RaisePropertyChanged("Ready");
            });

            if (!punchBand.Worn)
            {
                if (fitnessSensorsPunchBand == punchBand)
                {
                    fitnessSensorsPunchBand = null;
                }

                Debug.WriteLine("Stopping Fitness Sensors - Side:{0}", punchBand.FistSide);
                await StopFitnessSensors(punchBand.BandClient);
            }
            else
            {
                if (fitnessSensorsPunchBand == null)
                {
                    fitnessSensorsPunchBand = punchBand;
                    Debug.WriteLine("Starting Fitness Sensors - Side:{0}", punchBand.FistSide);
                    await StartFitnessSensors(punchBand.BandClient);
                }
            }
        }

        private async Task StartFitnessSensors(BandClient bandClient)
        {
            bandClient.SensorManager.Pedometer.ReadingChanged += PedometerOnReadingChanged;
            bandClient.SensorManager.HeartRate.ReadingChanged += HeartRateOnReadingChanged;
            bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperatureOnReadingChanged;
            
            if (bandClient.SensorManager.HeartRate.UserConsented != UserConsent.Granted)
            {
                await bandClient.SensorManager.HeartRate.RequestUserConsent();
            }
            
            await bandClient.SensorManager.HeartRate.StartReadingsAsync(BandSensorSampleRate.Ms128);
            await bandClient.SensorManager.Pedometer.StartReadingsAsync(BandSensorSampleRate.Ms128);
            await bandClient.SensorManager.SkinTemperature.StartReadingsAsync(BandSensorSampleRate.Ms128);
        }

        private async Task StopFitnessSensors(BandClient bandClient)
        {
            bandClient.SensorManager.Pedometer.ReadingChanged -= PedometerOnReadingChanged;
            bandClient.SensorManager.HeartRate.ReadingChanged -= HeartRateOnReadingChanged;
            bandClient.SensorManager.SkinTemperature.ReadingChanged -= SkinTemperatureOnReadingChanged;

            await bandClient.SensorManager.HeartRate.StopReadingsAsync();
            await bandClient.SensorManager.Pedometer.StopReadingsAsync();
            await bandClient.SensorManager.SkinTemperature.StopReadingsAsync();
        }

        private void PunchBandOnStartFight(object sender, EventArgs eventArgs)
        {
            invokeOnUiThread(() => StartFight(this, EventArgs.Empty));
        }

        private void PunchBandOnPunchInfoChanged(object sender, EventArgs eventArgs)
        {
            var punchBand = sender as PunchBand;
            var punchInfo = punchBand.PunchInfo;

            var punchEventArgs = new PunchEventArgs(punchInfo.FistSide, punchInfo.Strength, punchInfo.PunchRecognition);

            if (punchInfo.Status == PunchStatus.Finish && punchInfo.Strength.HasValue)
            {
                invokeOnUiThread(() => PunchEnded(this, punchEventArgs));
            }
            else if (punchInfo.Status == PunchStatus.Start)
            {
                invokeOnUiThread(() => PunchStarted(this, punchEventArgs));
            }
            else if (punchInfo.Status == PunchStatus.InProgress)
            {
                invokeOnUiThread(() => Punching(this, punchEventArgs));
            }
            
            if (punchInfo.PunchRecognition != PunchRecognition.Unknown)
            {
                invokeOnUiThread(() => PunchRecognized(this, punchEventArgs));
            }
        }

        private async void PunchBandOnFistSideSelected(object sender, EventArgs eventArgs)
        {
            var punchBand = sender as PunchBand;

            var otherPunchBand = punchBands.FirstOrDefault(p => p != punchBand);

            invokeOnUiThread(() =>
            {
                FistSides = punchBand.FistSide;
            });

            if (otherPunchBand != null)
            {
                await otherPunchBand.ForceFistSide(
                    punchBand.FistSide == FistSides.Left? FistSides.Right : FistSides.Left);
            }
        }

        public void Disconnect()
        {
            if (!Connected || punchBands == null || punchBands.Count == 0) return;

            CleanupBandClients();

            Connected = false;
            Worn = false;
        }

        private void CleanupBandClients()
        {
            foreach (var punchBand in punchBands)
            {
                punchBand.Dispose();
            }

            punchBands.Clear();
        }

        private void SkinTemperatureOnReadingChanged(object sender, BandSensorReadingEventArgs<BandSkinTemperatureReading> bandSensorReadingEventArgs)
        {
            invokeOnUiThread(() => SkinTemperature = (bandSensorReadingEventArgs.SensorReading.Temperature * 1.8) + 32.0);
        }

        private void HeartRateOnReadingChanged(object sender, BandSensorReadingEventArgs<BandHeartRateReading> bandSensorReadingEventArgs)
        {
            invokeOnUiThread(() =>
            {
                HeartRate = bandSensorReadingEventArgs.SensorReading.HeartRate;
                HeartRateLocked = bandSensorReadingEventArgs.SensorReading.Quality == HeartRateQuality.Locked;

                if (heartRate.HasValue)
                {
                    lastCalorieCountUpdate = lastCalorieCountUpdate ?? DateTime.UtcNow;

                    var calorieCount = CalorieCalculator.GetCalories(
                        userModel.Gender,
                        heartRate.Value,
                        userModel.Weight,
                        userModel.Age,
                        DateTime.UtcNow - lastCalorieCountUpdate.Value);

                    CalorieCount += calorieCount;
                    lastCalorieCountUpdate = DateTime.UtcNow;
                }
            });
        }

        private void PedometerOnReadingChanged(object sender, BandSensorReadingEventArgs<BandPedometerReading> bandSensorReadingEventArgs)
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

        public string TrainPunchType
        {
            get { return punchBands.Count > 0 ? punchBands.First().PunchDetector.TrainPunchType : PunchType.Jab.ToString(); }
            set
            {
                foreach (var punchBand in punchBands)
                {
                    punchBand.PunchDetector.TrainPunchType = value;
                }
            }
        }
    }
}
