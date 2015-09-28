using PunchingBand.Models;
using PunchingBand.Pages.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
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

            var historyModel = new HistoryModel(readStream, writeStream);
            var userModel = new UserModel();
            var punchingModel = new PunchingModel(userModel, Device.BeginInvokeOnMainThread, readStream, writeStream);
            var gameModel = new GameModel(punchingModel, historyModel, userModel, readStream, writeStream);

            var control = new GameEditor
            {
                WidthRequest = 350,
                HeightRequest = 350,
                BindingContext = gameModel,
            };


            absoluteLayout.Children.Add(control);

            Content = absoluteLayout;

            //this.Appearing += async (sender, args) => await control.Begin();
        }

    }
}
