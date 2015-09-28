
using Xamarin.Forms;

namespace PunchingBand.Pages.Views.Game
{
    public class StrengthMeter : ContentView
    {
        private Image meterGlow;
        private Image meterGradient;
        private BoxView meterGardientCover;
        private Label strengthLabel;

        private double value;
        private double maximum;

        public StrengthMeter()
        {
            meterGradient = new Image
            {
                Source = ImageSource.FromResource("PunchingBand.Resources.Images.StrengthMeterGradient.png"),
                Aspect = Aspect.Fill,
            };

            meterGardientCover = new BoxView
            {
                BackgroundColor = Color.Black,
            };

            meterGlow = new Image
            {
                Source = ImageSource.FromResource("PunchingBand.Resources.Images.StrengthMeterHalo.png"),
                Aspect = Aspect.Fill,
                Opacity = 0.1,
            };

            strengthLabel = new Label
            {
                TextColor = Color.FromHex("#FF909090"),
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
            };

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    strengthLabel,
                    new AbsoluteLayout
                    {
                        Children =
                        {
                            { meterGlow, new Rectangle(0.5, 0.5, 1.1, 2.2), AbsoluteLayoutFlags.All },
                            { meterGradient, new Rectangle(0.5, 0.5, 1, 1), AbsoluteLayoutFlags.All },
                            { meterGardientCover, new Rectangle(0.5, 0.5, 1, 1), AbsoluteLayoutFlags.All },
                        }
                    },
                }
            };

            Maximum = 8;
            Value = 0.1;
        }

        public double Maximum
        {
            get { return maximum; }
            set
            {
                this.maximum = value;
                UpdateMeter();
            }
        }

        public double Value
        {
            get { return value; }
            set
            {
                this.value = value;
                strengthLabel.Text = string.Format("{0:0.0}g", value);

                UpdateMeter();
            }
        }

        private void UpdateMeter()
        {
            var width = (value / Maximum);

            var rect = AbsoluteLayout.GetLayoutBounds(meterGardientCover);

            rect.Width = 1 - (width / 1);
            rect.X = width / (1 - rect.Width);
            

            AbsoluteLayout.SetLayoutBounds(meterGardientCover, rect);
        }
    }
}
