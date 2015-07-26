using System.Linq;
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

        public event EventHandler FistSideChanged = delegate { };

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
            if (propertyChangedEventArgs.PropertyName == "FistSide" && BandTile.FistSide != FistSide)
            {
                FistSide = BandTile.FistSide;
                FistSideChanged(this, EventArgs.Empty);

                await StartPunchDetection();
            }
        }

        private  async Task StartPunchDetection()
        {
            BandClient.SensorManager.Accelerometer.ReadingChanged += AccelerometerOnReadingChanged;

            BandClient.SensorManager.Accelerometer.ReportingInterval =
                BandClient.SensorManager.Accelerometer.SupportedReportingIntervals.Min();

            await PunchDetector.Initialize(FistSide);

            await BandClient.SensorManager.Accelerometer.StartReadingsAsync();
        }

        private void ContactOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandContactReading> bandSensorReadingEventArgs)
        {
            var worn = bandSensorReadingEventArgs.SensorReading.State == BandContactState.Worn;

            if (Worn != worn)
            {
                Worn = worn;
                WornChanged(this, EventArgs.Empty);
            }
        }

        private async void AccelerometerOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> bandSensorReadingEventArgs)
        {
            if (!Worn) return;

            PunchInfo = await PunchDetector.GetPunchInfo(bandSensorReadingEventArgs.SensorReading).ConfigureAwait(false);
            PunchInfoChanged(this, EventArgs.Empty);
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
    }
}
