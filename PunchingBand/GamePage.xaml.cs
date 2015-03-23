using System;
using System.ComponentModel;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PunchingBand
{
    public sealed partial class GamePage : Page
    {
        private readonly PunchingModel model;
        private readonly DispatcherTimer timer;
        private readonly SoundEffect punchSound;

        public GamePage()
        {
            InitializeComponent();

            model = App.Current.PunchingModel;
            DataContext = model;

            NavigationCacheMode = NavigationCacheMode.Required;

            model.PropertyChanged += ModelOnPropertyChanged;

            timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Start();

            punchSound = new SoundEffect("Assets/punch.wav");

            model.PunchStarted += ModelOnPunchStarted;
        }

        private void ModelOnPunchStarted(object sender, EventArgs eventArgs)
        {
            if (model.Running)
            {
                // TODO: predict punch strength for volume?
                punchSound.Play(1.0);
            }
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "PunchCount":
                    break;
                case "PunchStrength":
                    strengthMeterCover.Width = (1.0 - model.PunchStrength) * (strengthMeter.Width - 10);
                    if (model.Running)
                    {
                        //punchSound.Play(model.PunchStrength);
                    }
                    break;
                case "Running":
                    playButton.Visibility = model.Running ? Visibility.Collapsed : Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.

            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;

            // TODO: add 3, 2, 1 count down before game start

            model.StartGame();
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            model.StopGame();

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

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            model.StartGame();
        }

        private void TimerOnTick(object sender, object o)
        {
            model.Update();
        }
    }
}
