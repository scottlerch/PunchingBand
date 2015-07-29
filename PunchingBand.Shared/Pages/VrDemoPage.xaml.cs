using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PunchingBand.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VrDemoPage : Page
    {
        public VrDemoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
#endif
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
            var frame = Window.Current.Content as Frame;

            if (frame == null)
            {
                return;
            }

            if (frame.CanGoBack)
            {
                frame.GoBack();
            }

            backPressedEventArgs.Handled = true;
        }
#endif
    }
}
