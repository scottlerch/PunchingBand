using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace PunchingBand.Models
{
    public class BandTileModel
    {
        private static readonly Guid TileId = new Guid("66124A35-BFBF-43FD-A203-1A3E2CC0C458");

        private enum ElementId : short
        {
            TitleText = 1,
            Button = 2,
        }

        public async Task Initialize(IBandClient bandClient)
        {
            // NOTE: Resolution of Band is 320x106

            var textBlock = new TextBlock
            {
                ElementId = (short)ElementId.TitleText,
                Rect = new PageRect(0, 0, 200, 25)
            };

            var button = new TextButton()
            {
                ElementId = (short) ElementId.Button,
            };

            var panel = new FlowPanel(textBlock, button)
            {
                Orientation = FlowPanelOrientation.Vertical,
                Rect = new PageRect(0, 0, 250, 100)
            };

            var bandTile = new BandTile(TileId)
            {
                Name = "Punching Band",
                TileIcon = await LoadIcon("ms-appx:///Assets/Images/TileIconLarge.png"),
                SmallIcon = await LoadIcon("ms-appx:///Assets/Images/TileIconSmall.png")
            };

            bandTile.PageLayouts.Add(new PageLayout(panel));

            // Remove the Tile from the Band, if present. An application won't need to do this everytime it runs. 
            // But in case you modify this code and run it again, let's make sure to start fresh.
            await bandClient.TileManager.RemoveTileAsync(TileId);

            // Create the Tile on the Band.
            await bandClient.TileManager.AddTileAsync(bandTile);

            // And create the page with the specified texts and values.
            var page = new PageData(
                Guid.NewGuid(), // the Id for the page
                0,
                new TextBlockData((short)ElementId.TitleText, "PUNCHING BAND"),
                new TextButtonData((short)ElementId.Button, "Right Fist"));

            await bandClient.TileManager.SetPagesAsync(bandTile.TileId, page);
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
