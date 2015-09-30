using PunchingBand.Models;
using PunchingBand.Pages.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PunchingBand.Pages
{
    public class MiniGamePage : ContentPage
    {
        public MiniGamePage()
        {
            var absoluteLayout = new AbsoluteLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };

            var control = new HistoryListView
            {
                WidthRequest = 450,
                HeightRequest = 450,
                BindingContext = XamarinApp.RootModel.HistoryModel,
            };

            absoluteLayout.Children.Add(control);

            Content = absoluteLayout;

            //this.Appearing += async (sender, args) => await control.Begin();
        }

    }
}
