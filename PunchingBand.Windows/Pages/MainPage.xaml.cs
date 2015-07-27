using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace PunchingBand.Pages
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
    
            NavigationCacheMode = NavigationCacheMode.Required;

            DataContext = App.Current.RootModel;

            App.Current.RootModel.PunchingModel.StartFight += (sender, args) => Frame.Navigate(typeof(GamePage));
#if !DEBUG
            AdminButton.Visibility = Visibility.Collapsed;
#endif
        }

        public string StatusText
        {
            get { return statusBar.Text; }
            set { statusBar.Text = value; }
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (GamePage));
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdminPage));
        }
    }
}
