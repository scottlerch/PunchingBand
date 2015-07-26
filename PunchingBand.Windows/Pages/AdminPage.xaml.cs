using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PunchingBand.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminPage : Page
    {
        private int count = 0;

        public AdminPage()
        {
            InitializeComponent();

            DataContext = App.Current.RootModel;

            App.Current.RootModel.PunchingModel.PunchEnded += PunchingModel_PunchEnded;
            App.Current.RootModel.PunchingModel.Punching += PunchingModel_Punching;
            App.Current.RootModel.PunchingModel.PunchStarted += PunchingModel_PunchStarted;
            App.Current.RootModel.PunchingModel.PunchRecognized += PunchingModelPunchRecognized;
        }

        private void PunchingModelPunchRecognized(object sender, Models.PunchEventArgs e)
        {
            punchType.Text = e.PunchRecognition.PunchType.ToString();
            punchRecognitionDelay.Text = e.PunchRecognition.Delay.TotalMilliseconds.ToString("0.000") + "msec";
        }

        private void PunchingModel_Punching(object sender, Models.PunchEventArgs e)
        {
        }

        private void PunchingModel_PunchEnded(object sender, Models.PunchEventArgs e)
        {
        }

        private void PunchingModel_PunchStarted(object sender, Models.PunchEventArgs e)
        {
            count++;
            punchCount.Text = count.ToString();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }
    }
}
