using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PunchingBand.Utilities;

namespace PunchingBand.Pages
{
    public class ResponsivePage : Page
    {
        public ResponsivePage()
        {
            SizeChanged += ResponsivePage_SizeChanged;
            Unloaded += ResponsivePage_Unloaded;
        }

        private void ResponsivePage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= ResponsivePage_SizeChanged;
            Unloaded -= ResponsivePage_Unloaded;
        }

        private void ResponsivePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = DeviceInfo.GetActualResolution(e.NewSize.Width);

            if (width > 1170)
            {
                VisualStateManager.GoToState(this, "Large", true);
            }
            else if (width > 970)
            {
                VisualStateManager.GoToState(this, "Medium", true);
            }
            else if (width > 650)
            {
                VisualStateManager.GoToState(this, "Small", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "ExtraSmall", true);
            }
        }
    }
}
