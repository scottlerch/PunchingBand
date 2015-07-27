using Windows.UI.Xaml;

namespace PunchingBand.Pages.UserControls
{
    public sealed partial class UserSetup
    {
        public UserSetup()
        {
            InitializeComponent();

#if WINDOWS_PHONE_APP
            this.nameTextBlock.Visibility = Visibility.Collapsed;
            this.nameTextBox.Visibility = Visibility.Collapsed;
#endif
        }
    }
}
