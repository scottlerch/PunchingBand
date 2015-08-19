using Microsoft.Band.Portable;
using Microsoft.Band.Portable.Notifications;
using Microsoft.Band.Portable.Sensors;
using Microsoft.Band.Portable.Tiles;
using Microsoft.Band.Portable.Tiles.Pages;
using Microsoft.Band.Portable.Tiles.Pages.Data;
using PunchingBand.Recognition;
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

        private BandClient bandClient;
        private FistSides fistSide = FistSides.Unknown;

        public event EventHandler FightButtonClick = delegate { }; 

        public FistSides FistSide
        {
            get { return fistSide; }
            set { if (!Set("FistSide", ref fistSide, value)) RaisePropertyChanged("FistSide"); } // Always raise event
        }

        public async Task Initialize(BandClient bandClient)
        {
            this.bandClient = bandClient;

            var tile = await GetTile();

            bandClient.TileManager.TileButtonPressed += TileManagerOnTileButtonPressed;
            bandClient.SensorManager.Contact.ReadingChanged  += ContactOnReadingChanged;

            await SetPages(includeFistSelection: fistSide == FistSides.Unknown);

            await bandClient.TileManager.StartEventListenersAsync();
        }

        public async Task ForceFistSide(FistSides fistSide)
        {
            this.fistSide = fistSide;
            await SetPages(includeFistSelection: fistSide == FistSides.Unknown);
        }

        private async void ContactOnReadingChanged(object sender, BandSensorReadingEventArgs<BandContactReading> bandSensorReadingEventArgs)
        {
            if (bandSensorReadingEventArgs.SensorReading.State == ContactState.Worn)
            {
                await bandClient.NotificationManager.ShowDialogAsync(
                    TileId,
                    "Punching Band",
                    "Please specify fist.");
            }

            await SetPages(includeFistSelection: fistSide == FistSides.Unknown);
        }

        private async void TileManagerOnTileButtonPressed(object sender, BandTileButtonPressedEventArgs bandTileEventArgs)
        {
            switch ((ElementId)bandTileEventArgs.ElementId)
            {
                case ElementId.LeftFistButton:
                    FistSide = FistSides.Left;
                    PunchDetector.PrimaryFistSide = FistSides.Left;
                    await bandClient.NotificationManager.VibrateAsync(VibrationType.OneToneHigh);
                    await SetPages(includeFistSelection: false);
                    break;
                case ElementId.RightFistButton:
                    FistSide = FistSides.Right;
                    PunchDetector.PrimaryFistSide = FistSides.Right;
                    await bandClient.NotificationManager.VibrateAsync(VibrationType.OneToneHigh);
                    await SetPages(includeFistSelection: false);
                    break;
                case ElementId.FightButton:
                    FightButtonClick(this, EventArgs.Empty);
                    await bandClient.NotificationManager.VibrateAsync(VibrationType.OneToneHigh);
                    break;
            }
        }

        private async Task SetPages(bool includeFistSelection)
        {
            await bandClient.TileManager.RemoveTilePagesAsync(TileId);

            var fistSelectionPage = new PageData()
            {
                PageId = FistSelectionPageId,
                PageLayoutIndex = 0,
                Data =
                {
                     new TextBlockData() { ElementId = (short)ElementId.FistSelectionText, Text = "Which fist is this?" },
                new TextButtonData() { ElementId = (short)ElementId.LeftFistButton, Text = "Left" },
                new TextButtonData() { ElementId = (short)ElementId.RightFistButton, Text = "Right" }
                }
            };
             
            var titlePage = new PageData()
            {
                PageId = TitlePageId,
                PageLayoutIndex = 1,
                Data =
                {
                    new TextBlockData() { ElementId = (short)ElementId.TitleText, Text = "Punching Band" },
                    new TextButtonData() { ElementId = (short)ElementId.FightButton, Text = "FIGHT!" }
                }
            };

            if (includeFistSelection)
            {
                await bandClient.TileManager.SetTilePageDataAsync(TileId, fistSelectionPage);
            }
            else
            {
                await bandClient.TileManager.SetTilePageDataAsync(TileId, titlePage);
            }
        }

        private async Task<BandTile> GetTile()
        {
#if FORCE_TILE_REFRESH
            await bandClient.TileManager.RemoveTileAsync(TileId);
#endif

            var tiles = await bandClient.TileManager.GetTilesAsync();
            var tile = tiles.FirstOrDefault(t => t.Id == TileId);

            if (tile == null)
            {
                tile = new BandTile(TileId)
                {
                    Name = "Punching Band",
                    Icon = await LoadIcon("ms-appx:///Assets/Images/TileIconLarge.png"),
                    SmallIcon = await LoadIcon("ms-appx:///Assets/Images/TileIconSmall.png")
                };

                // NOTE: Resolution of page area is 245x106, recommended 15px margins on left and right

                var fightPage =
                    new PageLayout(
                        new FlowPanel()
                        {
                            Elements =
                            {
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
                                }
                            },
                            Orientation = FlowPanelOrientation.Vertical,
                            Rect = new PageRect(0, 0, 245, 100),
                            Margins = new Margins(0, 0, 0, 0),
                        }
                    );

                var fistSelectionPage =
                    new PageLayout(
                        new FlowPanel()
                        {
                            Elements =
                            {
                                 new TextBlock
                                {
                                    ElementId = (short)ElementId.FistSelectionText,
                                    Rect = new PageRect(0, 0, 200, 30),
                                    Margins = new Margins(15, 0, 0, 0),
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    VerticalAlignment = VerticalAlignment.Bottom,
                                },
                                new FlowPanel()
                                {
                                    Elements =
                                    {
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
                                        }
                                    },
                                    Orientation = FlowPanelOrientation.Horizontal,
                                    Rect = new PageRect(0, 0, 245, 50),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Margins = new Margins(0, 15, 0, 0)
                                }
                            },
                            Orientation = FlowPanelOrientation.Vertical,
                            Rect = new PageRect(0, 0, 245, 100),
                            Margins = new Margins(0, 0, 0, 0),
                        });

                tile.PageLayouts.Add(fistSelectionPage);
                tile.PageLayouts.Add(fightPage);

                await bandClient.TileManager.AddTileAsync(tile);
            }

            return tile;
        }

        private static async Task<BandImage> LoadIcon(string uri)
        {
            var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return BandImage.FromWriteableBitmap(bitmap);
            }
        }
    }
}
