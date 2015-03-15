﻿using Microsoft.Band;
using Microsoft.Band.Sensors;
using System;
using System.Linq;

namespace PunchingBand
{
    internal class PunchingModel : ModelBase
    {
        private IBandClient bandClient;
        private bool connected;
        private string status = "Connecting...";

        private readonly PunchDetector punchDetector = new PunchDetector();

        private int punchCount;
        private double punchStrength;
        private int? heartRate;
        private bool heartRateLocked;
        private double? skinTemperature;
        private long stepCount;
        private long? startStepCount;
        private bool worn;

        private readonly Action<Action> invokeOnUiThread;

        public PunchingModel()
        {
            invokeOnUiThread = action => action();
        }

        public PunchingModel(Action<Action> invokeOnUiThread)
        {
            this.invokeOnUiThread = invokeOnUiThread;
        }

        public int PunchCount
        {
            get { return punchCount; }
            set { Set("PunchCount", ref punchCount, value); }
        }

        public double PunchStrength
        {
            get { return punchStrength; }
            set { Set("PunchStrength", ref punchStrength, value); }
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

        public async void Connect()
        {
            if (Connected) return;

            try
            {
                var bands = await BandClientManager.Instance.GetBandsAsync();

                if (bands.Length > 0)
                {
                    bandClient = await BandClientManager.Instance.ConnectAsync(bands[0]);

                    bandClient.SensorManager.Accelerometer.ReadingChanged += AccelerometerOnReadingChanged;
                    bandClient.SensorManager.Contact.ReadingChanged += ContactOnReadingChanged;
                    bandClient.SensorManager.Pedometer.ReadingChanged += PedometerOnReadingChanged;
                    bandClient.SensorManager.HeartRate.ReadingChanged += HeartRateOnReadingChanged;
                    bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperatureOnReadingChanged;

                    bandClient.SensorManager.Accelerometer.ReportingInterval = bandClient.SensorManager.Accelerometer.SupportedReportingIntervals.First();

                    await bandClient.SensorManager.Accelerometer.StartReadingsAsync();
                    await bandClient.SensorManager.Contact.StartReadingsAsync();
                    await bandClient.SensorManager.HeartRate.StartReadingsAsync();
                    await bandClient.SensorManager.Pedometer.StartReadingsAsync();
                    await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();

                    Connected = true;

                    Status = "Band connected!";
                }
                else
                {
                    Status = "No Band found!";
                }
            }
            catch (Exception ex)
            {
                Status = "Error connecting to Band: " + ex.Message;

                if (bandClient != null)
                {
                    bandClient.Dispose();
                    bandClient = null;
                }
            }
        }

        public void Disconnect()
        {
            if (!Connected || bandClient == null) return;

            bandClient.Dispose();
            bandClient = null;

            punchDetector.Reset();

            Connected = false;
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
            invokeOnUiThread(() => Worn = bandSensorReadingEventArgs.SensorReading.State == BandContactState.Worn);
        }

        private void AccelerometerOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> bandSensorReadingEventArgs)
        {
            if (punchDetector.IsPunchDetected(bandSensorReadingEventArgs.SensorReading))
            {
                invokeOnUiThread(() =>
                {
                    PunchCount++;
                    PunchStrength = punchDetector.LastPunchStrength;
                });
            }
        }
    }
}
