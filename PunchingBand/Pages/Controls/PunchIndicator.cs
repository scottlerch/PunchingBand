using Xamarin.Forms;

namespace PunchingBand.Pages.Controls
{
    public class PunchIndicator : ContentView
    {
        // TODO: render combo indicator and punch type

        private int count;
        private string type;

        private Image fistImage;
        private Label punchCountLabel;
        private Label punchTypeLabel;

        public PunchIndicator()
        {
            punchCountLabel = new Label
            {
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)) * 5.0,
                XAlign = TextAlignment.Center,
                YAlign = TextAlignment.Center,
                Opacity = 0.5,
            };

            punchTypeLabel = new Label
            {
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)) * 2.0,
                XAlign = TextAlignment.Center,
                YAlign = TextAlignment.Center,
                Opacity = 1.0,
            };

            fistImage = new Image
            {
                Source = ImageSource.FromResource("PunchingBand.Resources.Images.Fist.png"),
                Aspect = Aspect.Fill,
                Opacity = 0.2,
            };

            Content = new AbsoluteLayout
            {
                Children =
                {
                    { fistImage, new Rectangle(0.5, 0.5, 1, 1), AbsoluteLayoutFlags.All },
                    { punchCountLabel, new Rectangle(0.5, 0.5, 1, 1), AbsoluteLayoutFlags.All },
                    { punchTypeLabel, new Rectangle(0.5, 0.5, 1, 1), AbsoluteLayoutFlags.All },
                },
            };

            Count = 0;
            Type = "";
        }

        public int Count
        {
            get { return count; }
            set
            {
                count = value;

                punchCountLabel.Text = count.ToString();
            }
        }

        public string Type
        {
            get { return type; }
            set
            {
                type = value;

                punchTypeLabel.Text = type;
            }
        }
    }
}
