﻿using Microsoft.Band;
using Microsoft.Band.Sensors;
using PunchingBand.Recognition;
using PunchingBand.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;

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
        private FistSides fitnessSensorsSide = FistSides.Unknown;

        private readonly Action<Action> invokeOnUiThread;

        public event EventHandler<PunchEventArgs> PunchStarted = delegate { };
        public event EventHandler<PunchEventArgs> Punching = delegate { };
        public event EventHandler<PunchEventArgs> PunchEnded = delegate { };
        public event EventHandler<PunchEventArgs> PunchRecognized = delegate { };
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
                for (int i = 0; i < Math.Min(bands.Length, 2); i++)
                {
                    var punchBand = new PunchBand(await BandClientManager.Instance.ConnectAsync(bands[i]));

                    punchBands.Add(punchBand);

                    punchBand.FistSideChanged += PunchBandOnFistSideChanged;
                    punchBand.PunchInfoChanged += PunchBandOnPunchInfoChanged;
                    punchBand.WornChanged += PunchBandOnWornChanged;
                    punchBand.StartFight += PunchBandOnStartFight;

                    await punchBand.Initialize();
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

            invokeOnUiThread(() =>
            {
                // Consider worn if either wrist has band worn
                Worn = punchBands.Any(b => b.Worn);
                Status = Worn ? string.Empty : "Please wear your Band.";
            });

            if (!punchBand.Worn && punchBand.FistSide == fitnessSensorsSide)
            {
                await StopFitnessSensors(punchBand.BandClient);
                fitnessSensorsSide = FistSides.Unknown;
            }
            else if (punchBand.Worn && fitnessSensorsSide == FistSides.Unknown)
            {
                await StartFitnessSensors(punchBand.BandClient);
                fitnessSensorsSide = punchBand.FistSide;
            }
        }

        private async Task StartFitnessSensors(IBandClient bandClient)
        {
            bandClient.SensorManager.Pedometer.ReadingChanged += PedometerOnReadingChanged;
            bandClient.SensorManager.HeartRate.ReadingChanged += HeartRateOnReadingChanged;
            bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperatureOnReadingChanged;
            
            if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() != UserConsent.Granted)
            {
                await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }
            
            await bandClient.SensorManager.HeartRate.StartReadingsAsync();
            await bandClient.SensorManager.Pedometer.StartReadingsAsync();
            await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();
        }

        private async Task StopFitnessSensors(IBandClient bandClient)
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

        private void PunchBandOnFistSideChanged(object sender, EventArgs eventArgs)
        {
            var punchBand = sender as PunchBand;

            invokeOnUiThread(() =>
            { 
                FistSides = punchBand.FistSide;
            });
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

                if (lastCalorieCountUpdate.HasValue && heartRate.HasValue)
                {
                    CalorieCount = CalorieCalculator.GetCalories(
                        userModel.Gender,
                        heartRate.Value,
                        userModel.Weight,
                        userModel.Age,
                        DateTime.UtcNow - lastCalorieCountUpdate.Value);
                }
                else
                {
                    lastCalorieCountUpdate = DateTime.UtcNow;
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
