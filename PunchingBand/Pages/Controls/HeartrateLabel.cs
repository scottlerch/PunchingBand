using Xamarin.Forms;

namespace PunchingBand.Pages.Controls
{
    class HeartRateLabel : ContentView
    {
        private double? heartrate;
        private Label valueLabel;
        private Image heartImage;

        private double maximumImageSize;
        private double minimumImageSize;

        public HeartRateLabel()
        {
            // TODO: get maximum height from size change event on Label so it's based on the font size
            maximumImageSize = 32.0;
            minimumImageSize = maximumImageSize / 2.0;

            valueLabel = new Label
            {
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                TextColor = Color.FromHex("#FF909090"),
            };

            heartImage = new Image
            {
                Source = ImageSource.FromResource("PunchingBand.Resources.Images.Heart.png"),
                Aspect = Aspect.Fill,
                Opacity = 0.5,
                WidthRequest = maximumImageSize,
                HeightRequest = maximumImageSize,
            };

            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    heartImage,
                    valueLabel
                }
            };

            Value = null;
        }

        public double? Value
        {
            get
            {
                return heartrate;
            }
            set
            {
                this.heartrate = value;
                valueLabel.Text = string.Format(" {0}", value.HasValue? value.Value.ToString("0") : "N/A");
            }
        }
    }
}
