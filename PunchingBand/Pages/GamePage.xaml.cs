﻿using System;
using System.ComponentModel;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PunchingBand.Infrastructure;
using PunchingBand.Models;

namespace PunchingBand.Pages
{
    public sealed partial class GamePage
    {
        private readonly RootModel model;
        private readonly SoundEffect punchSound;
        private readonly SoundEffect beepSound;
        private readonly SoundEffect endBuzzer;
        private readonly SoundEffect fightBell;

        public GamePage()
        {
            InitializeComponent();

            model = App.Current.RootModel;
            DataContext = model;

            NavigationCacheMode = NavigationCacheMode.Required;

            var timer = new DispatcherTimer();
            timer.Tick += GameTimerOnTick;
            timer.Interval = TimeSpan.FromMilliseconds(17);
            timer.Start();

            punchSound = new SoundEffect("Assets/punch.wav");
            beepSound = new SoundEffect("Assets/countdownbeep.wav");
            endBuzzer = new SoundEffect("Assets/endbuzzer.wav");
            fightBell = new SoundEffect("Assets/fightbell.wav");

            model.PunchingModel.PunchStarted += PunchingModelOnPunchStarted;
            model.GameModel.PropertyChanged += GameModelOnPropertyChanged;

            countDownUserControl.CountDownFinished += CountDownUserControlOnCountDownFinished;
            countDownUserControl.Loaded += CountDownUserControlOnLoaded;

            countDownGrid.Visibility = Visibility.Visible;
            gameGrid.Visibility = Visibility.Collapsed;
        }

        private void PunchingModelOnPunchStarted(object sender, EventArgs eventArgs)
        {
            if (model.GameModel.Running)
            {
                // TODO: predict punch strength for volume?
                punchSound.Play(1.0);
                punchCountStoryboard.Begin();
            }
        }

        private void GameModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "PunchCount":
                    break;
                case "PunchStrength":
                    strengthMeterCover.Width = (1.0 - model.GameModel.PunchStrength) * (strengthMeter.Width - 10);
                    if (model.GameModel.Running)
                    {
                        // NOTE: already played in ModelOnPunchStarted
                        //punchSound.Play(model.PunchStrength);
                    }
                    break;
                case "Running":
                    playButton.Visibility = model.GameModel.Running ? Visibility.Collapsed : Visibility.Visible;
                    if (!model.GameModel.Running)
                    {
                        endBuzzer.Play(0.3);
                    }
                    break;
                case "TimeLeftSeconds":
                    if (model.GameModel.TimeLeftSeconds <= 5)
                    {
                        countDownText.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                        beepSound.Play(0.08);
                    }
                    else
                    {
                        countDownText.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                    }
                    break;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.

            countDownGrid.Visibility = Visibility.Visible;
            gameGrid.Visibility = Visibility.Collapsed;

            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed; 
        }

        private void CountDownUserControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            countDownUserControl.Start();
        }

        private void CountDownUserControlOnCountDownFinished(object sender, EventArgs e)
        {
            countDownGrid.Visibility = Visibility.Collapsed;
            gameGrid.Visibility = Visibility.Visible;

            fightBell.Play(0.1);
            model.GameModel.StartGame();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            model.GameModel.StopGame();

            var frame = Window.Current.Content as Frame;

            if (frame == null)
            {
                return;
            }

            if (frame.CanGoBack)
            {
                frame.GoBack();
                backPressedEventArgs.Handled = true;
            }
        }

        private void RestartButtonOnClick(object sender, RoutedEventArgs e)
        {
            countDownGrid.Visibility = Visibility.Visible;
            gameGrid.Visibility = Visibility.Collapsed;

            countDownUserControl.Start();
        }

        private void GameTimerOnTick(object sender, object o)
        {
            model.GameModel.Update();
        }
    }
}
