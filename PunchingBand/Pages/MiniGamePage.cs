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

            Func<string, Task<Stream>> readStream = fileName => Task.FromResult(Stream.Null);
            Func<string, Task<Stream>> writeStream = fileName => Task.FromResult(Stream.Null);

            var root = new RootModel(Device.BeginInvokeOnMainThread, readStream, writeStream);
            root.Load();

            var control = new HistoryListView
            {
                WidthRequest = 400,
                HeightRequest = 400,
                BindingContext = root.HistoryModel,
            };

            absoluteLayout.Children.Add(control);

            Content = absoluteLayout;

            //this.Appearing += async (sender, args) => await control.Begin();
        }

    }
}
