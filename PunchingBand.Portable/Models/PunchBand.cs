using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Band.Portable;
using Microsoft.Band.Portable.Sensors;
using PunchingBand.Recognition;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace PunchingBand.Models
{
    public sealed class PunchBand : IDisposable
    {
        private bool punchDetectionRunning;
        private readonly Func<string, Task<Stream>> getReadStream;

        public PunchBand(BandClient bandClient, Func<string, Task<Stream>> getReadStream, Func<string, Task<Stream>> getWriteStream)
        {
            this.getReadStream = getReadStream;

            BandClient = bandClient;
            FistSide = FistSides.Unknown;
            Worn = false;
            PunchDetector = new PunchDetector(getReadStream, getWriteStream);
        }

        public BandClient BandClient { get; private set; }

        public PunchDetector PunchDetector { get; private set; }

        public BandTileModel BandTile { get; private set; }

        public FistSides FistSide { get; private set; }

        public bool Worn { get; private set; }

        public PunchInfo PunchInfo { get; private set; }

        public event EventHandler StartFight = delegate { };

        public event EventHandler FistSideSelected = delegate { };

        public event EventHandler WornChanged = delegate { };

        public event EventHandler PunchInfoChanged = delegate { };

        public async Task Initialize()
        {
            await SetupBandTile();

            BandClient.SensorManager.Contact.ReadingChanged += ContactOnReadingChanged;

            await BandClient.SensorManager.Contact.StartReadingsAsync(BandSensorSampleRate.Ms16);

            // Now wait until Band is worn to finish initialization...
        }

        private async Task SetupBandTile()
        {
            BandTile = new BandTileModel(getReadStream);

            BandTile.PropertyChanged += TileOnPropertyChanged;
            BandTile.FightButtonClick += (sender, args) => StartFight(this, EventArgs.Empty);

            await BandTile.Initialize(BandClient);
        }

        private async void TileOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "FistSide")
            {
                FistSide = BandTile.FistSide;
                FistSideSelected(this, EventArgs.Empty);

                await EnsurePunchDetection();
            }
        }

        private async void ContactOnReadingChanged(object sender, BandSensorReadingEventArgs<BandContactReading> bandSensorReadingEventArgs)
        {
            Worn = bandSensorReadingEventArgs.SensorReading.State == ContactState.Worn;
            WornChanged(this, EventArgs.Empty);

            await EnsurePunchDetection();
        }

        private async void GyroscopeOnReadingChanged(object sender, BandSensorReadingEventArgs<BandGyroscopeReading> bandSensorReadingEventArgs)
        {
            var accelerometerReading = lastAccelerometerReading;
            lastAccelerometerReading = null;

            if (!punchDetectionRunning) return;

            PunchInfo = await PunchDetector.GetPunchInfo(new Band.GyroscopeAccelerometerReading(bandSensorReadingEventArgs.SensorReading, accelerometerReading)).ConfigureAwait(false);
            PunchInfoChanged(this, EventArgs.Empty);
        }

        private BandAccelerometerReading lastAccelerometerReading = null;

        private async void AccelerometerOnReadingChanged(object sender, BandSensorReadingEventArgs<BandAccelerometerReading> bandSensorReadingEventArgs)
        {
            lastAccelerometerReading = bandSensorReadingEventArgs.SensorReading;
        }

        private async Task EnsurePunchDetection()
        {
            if (FistSide != FistSides.Unknown && Worn)
            {
                await StartPunchDetection();
            }
            else
            {
                await StopPunchDetection();
            }
        }

        private async Task StartPunchDetection()
        {
            if (punchDetectionRunning)
            {
                // If fist side changed after punch detection was running reinitialize for new fist side
                if (PunchDetector.FistSide != FistSide)
                {
                    await PunchDetector.Initialize(FistSide);
                }
                return;
            }

            var sw = Stopwatch.StartNew();

            BandClient.SensorManager.Gyroscope.ReadingChanged += GyroscopeOnReadingChanged;
            BandClient.SensorManager.Accelerometer.ReadingChanged += AccelerometerOnReadingChanged;

            await PunchDetector.Initialize(FistSide);
 
            await BandClient.SensorManager.Gyroscope.StartReadingsAsync(BandSensorSampleRate.Ms16);
            await BandClient.SensorManager.Accelerometer.StartReadingsAsync(BandSensorSampleRate.Ms16);

            Debug.WriteLine("Punch Detection Started: {0}", sw.Elapsed);

            punchDetectionRunning = true;
        }

        private async Task StopPunchDetection()
        {
            if (!punchDetectionRunning)
            {
                return;
            }

            BandClient.SensorManager.Gyroscope.ReadingChanged -= GyroscopeOnReadingChanged;

            await BandClient.SensorManager.Gyroscope.StopReadingsAsync();

            punchDetectionRunning = false;
        }

        public void Dispose()
        {
            FistSide = FistSides.Unknown;
            Worn = false;

            if (BandClient != null)
            {
                BandClient.DisconnectAsync();
                BandClient = null;
            }

            if (PunchDetector != null)
            {
                PunchDetector.Dispose();
                PunchDetector = null;
            }

            if (BandTile != null)
            {
                BandTile = null;
            }
        }

        public async Task ForceFistSide(FistSides fistSide)
        {
            FistSide = fistSide;
            await BandTile.ForceFistSide(fistSide);
            await EnsurePunchDetection();
        }
    }
}
