using System;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace PunchingBand.Pages.UserControls
{
    public sealed partial class HistoryGrid
    {
        public HistoryGrid()
        {
            InitializeComponent();

            UpdateGridBasedOnGameMode();

            if (!DesignMode.DesignModeEnabled)
            {
                App.Current.RootModel.GameModel.PropertyChanged += GameModelOnPropertyChanged;
            }
        }

        private void GameModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "GameMode")
            {
                UpdateGridBasedOnGameMode();
            }
        }

        private void UpdateGridBasedOnGameMode()
        {
            var gameMode = GameMode.MiniGame;

            if (!DesignMode.DesignModeEnabled)
            {
                gameMode = App.Current.RootModel.GameModel.GameMode;
            }

            if (gameMode == GameMode.MiniGame)
            {
#if WINDOWS_PHONE_APP
                windowsGameListView.Visibility = Visibility.Collapsed;
                phoneGameListView.Visibility = Visibility.Visible;
                windowsFitnessListView.Visibility = Visibility.Collapsed;
                phoneFitnessListView.Visibility = Visibility.Collapsed;
#else
                windowsGameListView.Visibility = Visibility.Visible;
                phoneGameListView.Visibility = Visibility.Collapsed;
                windowsFitnessListView.Visibility = Visibility.Collapsed;
                phoneFitnessListView.Visibility = Visibility.Collapsed;
#endif
            }
            else // assume fitness related mode
            {
#if WINDOWS_PHONE_APP
                windowsGameListView.Visibility = Visibility.Collapsed;
                phoneGameListView.Visibility = Visibility.Collapsed;
                windowsFitnessListView.Visibility = Visibility.Collapsed;
                phoneFitnessListView.Visibility = Visibility.Visible;
#else
                windowsGameListView.Visibility = Visibility.Collapsed;
                phoneGameListView.Visibility = Visibility.Collapsed;
                windowsFitnessListView.Visibility = Visibility.Visible;
                phoneFitnessListView.Visibility = Visibility.Collapsed;
#endif     
            }

        }
    }
}
