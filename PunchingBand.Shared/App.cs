﻿using System.Linq;
using PunchingBand.Infrastructure;
using PunchingBand.Models;
using PunchingBand.Pages;
using System;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using PunchingBand.Utilities;
using Windows.Storage;
using System.IO;
#if WINDOWS_PHONE_APP
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
#else
using Windows.UI.Notifications;
#endif

namespace PunchingBand
{
    public delegate void ApplicationActivatedEventHandler(IActivatedEventArgs e);

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App
    {
        public event ApplicationActivatedEventHandler Activated;

#if !XAMARIN_APP
        private readonly SoundEffect wornSoundEffect;
#endif
        public RootModel RootModel { get; private set; }

        private CoreDispatcher dispatcher;

#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            PortableDesignMode.DesignModeEnabled = false;

            InitializeComponent();
            Suspending += OnSuspending;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }

#if !XAMARIN_APP
            RootModel = new RootModel(
                InvokeOnUIThread, 
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

            RootModel.PunchingModel.PropertyChanged += PunchingModelOnPropertyChanged;

            wornSoundEffect = new SoundEffect("Assets/Audio/worn.wav");
#endif
        }

#if !XAMARIN_APP
        private async void UpdateStatus()
        {
            var statusText = RootModel.PunchingModel.Status;

#if WINDOWS_PHONE_APP && !XAMARIN_APP
            var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();

            if (string.IsNullOrWhiteSpace(statusText))
            {
                await statusBar.HideAsync();
            }
            else
            {
                statusBar.ProgressIndicator.Text = statusText;
                statusBar.ProgressIndicator.ProgressValue = null;
                await statusBar.ProgressIndicator.ShowAsync();
                await statusBar.ShowAsync();
            }
#else

            /*
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(statusText));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
             */
            if (Window.Current.Content != null)
            {
                var page = (Window.Current.Content as Frame).Content as MainPage;
                var app = Application.Current;
                if (page != null)
                {
                    page.StatusText = statusText;
                }
            }
            await Task.Yield();
#endif
        }
#endif

#if WINDOWS_PHONE_APP && !XAMARIN_APP
        public FileOpenPickerContinuationEventArgs FilePickerContinuationArgs { get; set; }
#endif

        public new static App Current
        {
            get { return (App)Application.Current; }
        }

        private void InvokeOnUIThread(Action action)
        {
            if (dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(action)).AsTask().Wait();
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = false;
            }
#endif

#if !XAMARIN_APP
            UpdateStatus();

            RootModel.PunchingModel.PropertyChanged += PunchingModelOnPropertyChanged;
#endif

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

#if WINDOWS_APP
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
#endif

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

#if XAMARIN_APP
                global::Xamarin.Forms.Forms.Init(e);
#endif
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter

#if XAMARIN_APP
                var mainPageType = typeof(XamarinMainPage);
#else
                var mainPageType = typeof(MainPage);
#endif

                if (!rootFrame.Navigate(mainPageType, e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            // Ensure the current window is active
            Window.Current.Activate();

#if !XAMARIN_APP
            await RootModel.Load();
            await RootModel.PunchingModel.Connect();
#endif
        }

#if !XAMARIN_APP
        private void PunchingModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "Status":
                    InvokeOnUIThread(UpdateStatus);
                    break;
                case "Worn":
                    if (RootModel.PunchingModel.Worn)
                    {
                        wornSoundEffect.Play(0.3);
                    }
                    break;
            }
        }
#endif

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            if (rootFrame != null)
            {
                rootFrame.ContentTransitions = transitions ?? new TransitionCollection { new NavigationThemeTransition() };
                rootFrame.Navigated -= RootFrame_FirstNavigated;
            }
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
#if !XAMARIN_APP
            RootModel.PunchingModel.Disconnect();
#endif

            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        protected async override void OnActivated(IActivatedEventArgs e)
        {
#if WINDOWS_PHONE_APP && !XAMARIN_APP
            var filePickerContinuationArgs = e as FileOpenPickerContinuationEventArgs;
            if (filePickerContinuationArgs != null)
            {
                FilePickerContinuationArgs = filePickerContinuationArgs;

                // HACK: this doesn't belong here, should be on page that actually cares about it
                if (FilePickerContinuationArgs != null && (string)FilePickerContinuationArgs.ContinuationData["Operation"] == "UpdateGameSong")
                {
                    RootModel.GameModel.Song = FilePickerContinuationArgs.Files.FirstOrDefault();
                }
            }
#endif
            if (Activated != null)
            {
                Activated(e);
            }

#if !XAMARIN_APP
            await RootModel.PunchingModel.Connect();
#endif
        }


        public static void NavigatetoGame(Frame frame)
        {
#if !XAMARIN_APP
            if (Current.RootModel.GameModel.VrEnabled)
            {
                frame.Navigate(typeof(VrDemoPage));
                return;
            }

            switch (Current.RootModel.GameModel.GameMode)
            {
                case GameMode.FreeformWorkout:
                case GameMode.GuidedWorkout:
                    frame.Navigate(typeof (WorkoutPage));
                    break;
                case GameMode.MiniGame:
                    frame.Navigate(typeof (GamePage));
                    break;
            }
#endif
        }
    }
}
