using System;
using System.Collections.Generic;
using System.ComponentModel;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PunchingBand.Infrastructure;
using PunchingBand.Models;
using Windows.Storage;
using PunchingBand.Recognition;

namespace PunchingBand.Pages
{
    public sealed partial class WorkoutPage
    {
        private readonly RootModel model;
        private readonly SoundEffect powerPunchSound;
        private readonly List<SoundEffect> punchSounds; 
        private readonly SoundEffect beepSound;
        private readonly SoundEffect endBuzzer;
        private readonly SoundEffect fightBell;
        private readonly SoundEffect highScoreSound;

        private MediaElement songMedia;

        private readonly Random random = new Random();

        public WorkoutPage()
        {
            InitializeComponent();

            model = App.Current.RootModel;
            DataContext = model;

            NavigationCacheMode = NavigationCacheMode.Required;

            var timer = new DispatcherTimer();
            timer.Tick += GameTimerOnTick;
            timer.Interval = TimeSpan.FromMilliseconds(17);
            timer.Start();

            punchSounds = new List<SoundEffect>
            {
                new SoundEffect("Assets/Audio/punch1.wav", poolSize: 3),
                new SoundEffect("Assets/Audio/punch2.wav", poolSize: 3),
                new SoundEffect("Assets/Audio/punch3.wav", poolSize: 3),
                new SoundEffect("Assets/Audio/punch4.wav", poolSize: 3),
            };

            powerPunchSound = new SoundEffect("Assets/Audio/heavypunch1.wav", poolSize: 10);
            beepSound = new SoundEffect("Assets/Audio/ding.wav", poolSize: 2);
            endBuzzer = new SoundEffect("Assets/Audio/endgame.wav");
            fightBell = new SoundEffect("Assets/Audio/fightbell.wav");
            highScoreSound = new SoundEffect("Assets/Audio/highscore.wav");

            model.PunchingModel.PunchStarted += PunchingModelOnPunchStarted;
            model.PunchingModel.PropertyChanged += PunchingModelOnPropertyChanged;
            model.GameModel.PropertyChanged += GameModelOnPropertyChanged;
            model.GameModel.GameEnded += GameModelOnGameEnded;

            countDownUserControl.CountDownFinished += CountDownUserControlOnCountDownFinished;
            countDownUserControl.Loaded += CountDownUserControlOnLoaded;

            countDownGrid.Visibility = Visibility.Visible;
            gameGrid.Visibility = Visibility.Collapsed;
            endGameTextBlock.Visibility = Visibility.Collapsed;

#if WINDOWS_PHONE_APP
            backButton.Visibility = Visibility.Collapsed;
#endif

            model.PunchingModel.StartFight += (sender, args) => Restart();
            heartScaleXAnimation.Completed += HeartScaleXAnimationOnCompleted;
        }

        private TimeSpan heartRateAnimationDuration = TimeSpan.MaxValue;

        private void HeartScaleXAnimationOnCompleted(object sender, object e)
        {
            heartScaleXAnimation.Duration = heartRateAnimationDuration;
            heartScaleYAnimation.Duration = heartRateAnimationDuration;
        }

        private void PunchingModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "HeartRate":
                    if (!model.PunchingModel.HeartRate.HasValue)
                    {
                        heartScaleXAnimation.Duration = TimeSpan.MaxValue;
                        heartScaleYAnimation.Duration = TimeSpan.MaxValue;
                    }
                    else
                    {
                        heartRateAnimationDuration = TimeSpan.FromSeconds((60.0 / model.PunchingModel.HeartRate.Value) / 2.0);
                    }
                    break;
            }
        }

        private void GameModelOnGameEnded(object sender, EventArgs eventArgs)
        {
            StopMusic();

            if (model.GameModel.NewHighScore)
            {
                endGameTextBlock.Text = "HIGH SCORE";
                endGameTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Yellow);
                endGameTextStoryBoard.Begin();
                highScoreSound.Play(1.0);
            }
            else
            {
                endGameTextBlock.Text = "GAME OVER";
                endGameTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                endGameTextStoryBoard.Begin();
                endBuzzer.Play(0.3);
            }

            endGameTextBlock.Visibility = Visibility.Visible;
        }

        private void PunchingModelOnPunchStarted(object sender, EventArgs eventArgs)
        {
            if (model.GameModel.Running)
            {
                // TODO: predict punch strength for volume?
                punchSounds[random.Next(punchSounds.Count)].Play(1.0);
                punchCountStoryboard.Begin();
                fistStoryboard.Begin();
            }
        }

        private void GameModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "PunchCount":
                    break;
                case "PunchStrength":
                    if (model.GameModel.Running)
                    {
                        if (PunchDetector.MaximumAcceleration - model.GameModel.PunchStrength < 0.001)
                        {
                            powerPunchSound.Play(1);
                            powerGlowStoryboard.Begin();
                        }
                    }
                    break;
                case "Running":
                    if (model.GameModel.Running)
                    {
                        punchTypeTextBlock.Visibility = Visibility.Visible;
                        powerComboTextBlock.Visibility = Visibility.Visible;
                        speedComboTextBlock.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        punchTypeTextBlock.Visibility = Visibility.Collapsed;
                        powerComboTextBlock.Visibility = Visibility.Collapsed;
                        speedComboTextBlock.Visibility = Visibility.Collapsed;
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
                case "SpeedComboText":
                    speedComboStoryboard.Begin();
                    break;
                case "PowerComboText":
                    powerComboStoryboard.Begin();
                    break;
                case "PunchType":
                    punchTypeStoryboard.Begin();
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
            punchCountTextBlock.Opacity = 0;

            countDownGrid.Visibility = Visibility.Visible;
            gameGrid.Visibility = Visibility.Collapsed;
            endGameTextBlock.Visibility = Visibility.Collapsed;

#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed; 
#endif
        }

        private void CountDownUserControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            countDownUserControl.Start();
        }

        private async void CountDownUserControlOnCountDownFinished(object sender, EventArgs e)
        {
            countDownGrid.Visibility = Visibility.Collapsed;
            gameGrid.Visibility = Visibility.Visible;

            fightBell.Play(0.1);
            model.GameModel.StartGame();

            if (model.GameModel.Song != null)
            {
                var stream = await (model.GameModel.Song as StorageFile).OpenAsync(FileAccessMode.Read);

                songMedia = new MediaElement {AudioCategory = AudioCategory.ForegroundOnlyMedia};
                songMedia.Loaded += (o, args) =>
                {
                    songMedia.SetSource(stream, (model.GameModel.Song as StorageFile).ContentType);
                    songMedia.IsLooping = true;
                    songMedia.Volume = 0.3;
                    songMedia.Play();
                };

                MainGrid.Children.Add(songMedia);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
#endif
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            Stop();
            backPressedEventArgs.Handled = true;
        }
#endif

        private void RestartButtonOnClick(object sender, RoutedEventArgs e)
        {
            Restart();
        }

        private void StopMusic()
        {
            if (songMedia != null)
            {
                try
                {
                    songMedia.IsMuted = true;
                    songMedia.Stop();
                    if (MainGrid.Children.Contains(songMedia))
                    {
                        MainGrid.Children.Remove(songMedia);
                    }
                    songMedia = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        private void Restart()
        {
            StopMusic();

            countDownGrid.Visibility = Visibility.Visible;
            gameGrid.Visibility = Visibility.Collapsed;
            endGameTextBlock.Visibility = Visibility.Collapsed;

            countDownUserControl.Start();
        }

        private void GameTimerOnTick(object sender, object o)
        {
            model.GameModel.Update();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
           Stop();
        }

        private void Stop()
        {
            StopMusic();

            countDownUserControl.Stop();
            model.GameModel.AbortGame();

            var frame = Window.Current.Content as Frame;

            if (frame == null)
            {
                return;
            }

            if (frame.CanGoBack)
            {
                frame.GoBack();
            }
        }
    }
}
