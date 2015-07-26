using Microsoft.Band;
using Microsoft.Band.Notifications;
using Microsoft.Band.Sensors;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace PunchingBand.Models
{
    public class BandTileModel : ModelBase
    {
        private static readonly Guid TileId = new Guid("66124A35-BFBF-43FD-A203-2A3E2CC0C558");
        private static readonly Guid FistSelectionPageId = new Guid("FAFF2B4C-773E-48DA-8446-0D4EE4C205DB");
        private static readonly Guid TitlePageId = new Guid("FAFF2B4C-773E-48DA-8446-0D4EE4C215DB");

        private enum ElementId : short
        {
            TitleText = 1,
            FistSelectionText = 2,
            LeftFistButton = 3,
            RightFistButton = 4,
            FightButton = 5,
        }

        private IBandClient bandClient;
        private FistSides fistSide = FistSides.Unknown;

        public event EventHandler FightButtonClick = delegate { }; 

        public FistSides FistSide
        {
            get { return fistSide; }
            set { Set("FistSide", ref fistSide, value); }
        }

        public async Task Initialize(IBandClient bandClient)
        {
            this.bandClient = bandClient;

            var tile = await GetTile();

            bandClient.TileManager.TileButtonPressed += TileManagerOnTileButtonPressed;
            bandClient.SensorManager.Contact.ReadingChanged  += ContactOnReadingChanged;

            await SetPages(includeFistSelection: true);

            await bandClient.TileManager.StartReadingsAsync();
        }

        private async void ContactOnReadingChanged(object sender, BandSensorReadingEventArgs<IBandContactReading> bandSensorReadingEventArgs)
        {
            if (bandSensorReadingEventArgs.SensorReading.State == BandContactState.Worn)
            {
                await bandClient.NotificationManager.ShowDialogAsync(
                    TileId,
                    "Punching Band",
                    "Please specify fist.");
            }
        }

        private async void TileManagerOnTileButtonPressed(object sender, BandTileEventArgs<IBandTileButtonPressedEvent> bandTileEventArgs)
        {
            switch ((ElementId)bandTileEventArgs.TileEvent.ElementId)
            {
                case ElementId.LeftFistButton:
                    FistSide = FistSides.Left;
                    await bandClient.NotificationManager.VibrateAsync(VibrationType.OneToneHigh);
                    break;
                case ElementId.RightFistButton:
                    FistSide = FistSides.Right;
                    await bandClient.NotificationManager.VibrateAsync(VibrationType.OneToneHigh);
                    break;
                case ElementId.FightButton:
                    FightButtonClick(this, EventArgs.Empty);
                    await bandClient.NotificationManager.VibrateAsync(VibrationType.OneToneHigh);
                    break;
            }
        }

        private async Task SetPages(bool includeFistSelection)
        {
            await bandClient.TileManager.RemovePagesAsync(TileId);

            var titlePage = new PageData(
                TitlePageId,
                0,
                new TextBlockData((short)ElementId.TitleText, "Punching Band"),
                new TextButtonData((short)ElementId.FightButton, "FIGHT!"));

            var fistSelectionPage = new PageData(
                FistSelectionPageId,
                1,
                new TextBlockData((short)ElementId.FistSelectionText, "Which fist is this?"),
                new TextButtonData((short)ElementId.LeftFistButton, "Left"),
                new TextButtonData((short)ElementId.RightFistButton, "Right"));

            if (includeFistSelection)
            {
                await bandClient.TileManager.SetPagesAsync(TileId, fistSelectionPage, titlePage);
            }
            else
            {
                await bandClient.TileManager.SetPagesAsync(TileId, titlePage);
            }
        }

        private async Task<BandTile> GetTile()
        {
#if FORCE_TILE_REFRESH
            await bandClient.TileManager.RemoveTileAsync(TileId);
#endif

            var tiles = await bandClient.TileManager.GetTilesAsync();
            var tile = tiles.FirstOrDefault(t => t.TileId == TileId);

            if (tile == null)
            {
                tile = new BandTile(TileId)
                {
                    Name = "Punching Band",
                    TileIcon = await LoadIcon("ms-appx:///Assets/Images/TileIconLarge.png"),
                    SmallIcon = await LoadIcon("ms-appx:///Assets/Images/TileIconSmall.png")
                };

                // NOTE: Resolution of page area is 245x106, recommended 15px margins on left and right

                tile.PageLayouts.Add(
                    new PageLayout(
                        new FlowPanel(
                            new TextBlock
                            {
                                ElementId = (short)ElementId.TitleText,
                                Rect = new PageRect(0, 0, 215, 30),
                                Margins = new Margins(15, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Bottom,
                            },
                            new TextButton
                            {
                                ElementId = (short)ElementId.FightButton,
                                Rect = new PageRect(0, 0, 215, 50),
                                Visible = true,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                Margins = new Margins(15, 15, 0, 0),
                            })
                        {
                            Orientation = FlowPanelOrientation.Vertical,
                            Rect = new PageRect(0, 0, 245, 100),
                            Margins = new Margins(0, 0, 0, 0),
                        }
                    ));

                tile.PageLayouts.Add(
                    new PageLayout(
                        new FlowPanel(
                            new TextBlock
                            {
                                ElementId = (short)ElementId.FistSelectionText,
                                Rect = new PageRect(0, 0, 200, 30),
                                Margins = new Margins(15, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Bottom,
                            },
                            new FlowPanel(
                                new TextButton
                                {
                                    ElementId = (short)ElementId.LeftFistButton,
                                    Rect = new PageRect(0, 0, 100, 50),
                                    Margins = new Margins(15, 0, 0, 0),
                                    Visible = true,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Bottom,
                                }, 
                                new TextButton
                                {
                                    ElementId = (short)ElementId.RightFistButton,
                                    Rect = new PageRect(0, 0, 100, 50),
                                    Visible = true,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Bottom,
                                    Margins = new Margins(10, 0, 0, 0),
                                })
                            {
                                Orientation = FlowPanelOrientation.Horizontal,
                                Rect = new PageRect(0, 0, 245, 50),
                                VerticalAlignment = VerticalAlignment.Center,
                                Margins = new Margins(0, 15, 0, 0)
                            })
                        {
                            Orientation = FlowPanelOrientation.Vertical,
                            Rect = new PageRect(0, 0, 245, 100),
                            Margins = new Margins(0, 0, 0, 0),
                        }));

                await bandClient.TileManager.AddTileAsync(tile);
            }

            return tile;
        }

        private static async Task<BandIcon> LoadIcon(string uri)
        {
            var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }
    }
}
