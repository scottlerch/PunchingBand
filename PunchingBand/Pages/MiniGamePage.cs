using PunchingBand.Models;
using PunchingBand.Pages.Views;
using PunchingBand.Pages.Views.Game;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PunchingBand.Pages
{
    public class MiniGamePage : ContentPage
    {
        private RootModel model;

        private CountDownLabel countDownLabel = new CountDownLabel();
        private PunchIndicator punchIndicator = new PunchIndicator();
        private DurationLabel durationLabel = new DurationLabel();
        private HeartRateLabel heartRateLabel = new HeartRateLabel();
        private ScoreLabel scoreLabel = new ScoreLabel();
        private StrengthMeter strengthMeter = new StrengthMeter();
        private TemperatureLabel temperatureLabel = new TemperatureLabel();

        public MiniGamePage()
        {
            model = XamarinApp.RootModel;

            BackgroundColor = Color.Black.AddLuminosity(0.1);

            Content = countDownLabel;

            Appearing += async (sender, args) =>
            {
                await countDownLabel.CountDown();

                temperatureLabel.HorizontalOptions = LayoutOptions.EndAndExpand;
                heartRateLabel.HorizontalOptions = LayoutOptions.Start;

                var topBar = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Padding = new Thickness(4, 4, 4, 4),
                    Children =
                    {
                        heartRateLabel,
                        temperatureLabel,
                    }
                };

                scoreLabel.HorizontalOptions = LayoutOptions.Center;
                punchIndicator.HorizontalOptions = LayoutOptions.Center;
                strengthMeter.HorizontalOptions = LayoutOptions.Center;
                durationLabel.HorizontalOptions = LayoutOptions.Center;
                durationLabel.Padding = new Thickness(30, 0, 0, 0);

                var mainScoreboard = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Children =
                    {
                        scoreLabel,
                        punchIndicator,
                        strengthMeter,
                        durationLabel,
                    }
                };

                Content = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    Children =
                    {
                        topBar,
                        mainScoreboard,
                    }
                };

                model.GameModel.StartGame();

                Device.StartTimer(TimeSpan.FromMilliseconds(17), () =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        model.GameModel.Update();
                        durationLabel.Value = model.GameModel.Duration;
                        punchIndicator.Count = model.GameModel.PunchCount;
                    });

                    return true;
                });
            };
        }
    }
}
