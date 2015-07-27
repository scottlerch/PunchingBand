using Windows.UI.Xaml;

namespace PunchingBand.Pages.UserControls
{
    public sealed partial class HistoryGrid
    {
        public HistoryGrid()
        {
            InitializeComponent();

#if WINDOWS_PHONE_APP
            windowsListView.Visibility = Visibility.Collapsed;
            phoneListView.Visibility = Visibility.Visible;
#else
            windowsListView.Visibility = Visibility.Visible;
            phoneListView.Visibility = Visibility.Collapsed;
#endif
        }
    }
}
