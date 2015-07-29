using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using PunchingBand.Models;

namespace PunchingBand.Pages.UserControls
{
    public sealed partial class GameSetup
    {
        public GameSetup()
        {
            InitializeComponent();

#if WINDOWS_APP
            if (!DesignMode.DesignModeEnabled)
            {
                App.Current.RootModel.GameModel.VrEnabled = false;
                vrCheckBox.IsEnabled = false;
            }
#endif
        }

        private async Task SelectSong()
        {
            var filePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.MusicLibrary,
                SettingsIdentifier = "FileOpenPicker",
            };

            filePicker.FileTypeFilter.Add(".mp3");

#if WINDOWS_APP
            var file = await filePicker.PickSingleFileAsync();

            (DataContext as GameModel).Song = file;
#else
            // TODO: implement rest of continuation pattern: https://msdn.microsoft.com/en-us/library/windows/apps/dn642086(v=vs.105).aspx
            filePicker.ContinuationData["Operation"] = "UpdateGameSong";
            filePicker.PickSingleFileAndContinue();

            await Task.Yield();
#endif
        }

        private async void Button_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await SelectSong();
        }
    }
}
