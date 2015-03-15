using System;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PunchingBand
{
    public sealed partial class MainPage : Page
    {
        private PunchingModel model;
        private MediaElement[] punchSounds;
        private DispatcherTimer timer;

        public MainPage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Start();

            model = new PunchingModel(InvokeOnUIThread);
            DataContext = model;

            NavigationCacheMode = NavigationCacheMode.Required;

            App.Current.Suspending += AppOnSuspending;
            App.Current.Resuming += AppOnResuming;
            App.Current.Activated += AppOnActivated;

            punchSounds = new[] { punchSound1, punchSound2, punchSound3 };

            model.PropertyChanged += ModelOnPropertyChanged;
        }

        private void InvokeOnUIThread(Action action)
        {
            if (Dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.High, delegate() { action(); });
            }
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "PunchCount":
                    punchSounds[model.PunchCount % punchSounds.Length].Volume = model.PunchStrength;
                    punchSounds[model.PunchCount % punchSounds.Length].Play();
                    break;
                case "PunchStrength":
                    strengthMeterCover.Width = (1.0-model.PunchStrength)*(strengthMeter.Width - 10);
                    break;
                case "Running":
                    if (model.Running)
                    {
                        playButton.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        playButton.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }

        private void AppOnActivated(IActivatedEventArgs activatedEventArgs)
        {
            model.Connect();
        }

        private void AppOnResuming(object sender, object o)
        {
            model.Connect();
        }

        private void AppOnSuspending(object sender, SuspendingEventArgs e)
        {
            model.Disconnect();
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

            model.Connect();
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
