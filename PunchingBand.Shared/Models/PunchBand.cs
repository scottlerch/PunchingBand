using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Band;
using Microsoft.Band.Sensors;
using PunchingBand.Recognition;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace PunchingBand.Models
{
    public sealed class PunchBand : IDisposable
    {
        private bool punchDetectionRunning;

        public PunchBand(IBandClient bandClient)
        {
            BandClient = bandClient;
            FistSide = FistSides.Unknown;
            Worn = false;
            PunchDetector = new PunchDetector(
                async filePath =>
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///" + filePath));
                    return await file.OpenStreamForReadAsync();
                },
                async filePath =>
                {
                    var folder = Path.GetDirectoryName(filePath);
                    var fileName = Path.GetFileName(filePath);

                    var local = ApplicationData.Current.LocalFolder;
                    var dataFolder = await local.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
                    var file = await dataFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                    return await file.OpenStreamForWriteAsync();
                });
        }

        public IBandClient BandClient { get; private set; }

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

            await BandClient.SensorManager.Contact.StartReadingsAsync();

            // Now wait until Band is worn to finish initialization...
        }

        private async Task SetupBandTile()
        {
            BandTile = new BandTileModel();

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

        private async void ContactOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandContactReading> bandSensorReadingEventArgs)
        {
            Worn = bandSensorReadingEventArgs.SensorReading.State == BandContactState.Worn;
            WornChanged(this, EventArgs.Empty);

            await EnsurePunchDetection();
        }

        private async void GyroscopeOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandGyroscopeReading> bandSensorReadingEventArgs)
        {
            if (!punchDetectionRunning) return;

            PunchInfo = await PunchDetector.GetPunchInfo(bandSensorReadingEventArgs.SensorReading).ConfigureAwait(false);
            PunchInfoChanged(this, EventArgs.Empty);
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

            BandClient.SensorManager.Gyroscope.ReadingChanged += GyroscopeOnReadingChanged;

            BandClient.SensorManager.Gyroscope.ReportingInterval =
                BandClient.SensorManager.Accelerometer.SupportedReportingIntervals.Min();

            await PunchDetector.Initialize(FistSide);

            await BandClient.SensorManager.Gyroscope.StartReadingsAsync();

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
                BandClient.Dispose();
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
