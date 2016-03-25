using Xamarin.Forms;

namespace PunchingBand.Pages.Views.Game
{
    public class ScoreLabel : ContentView
    {
        private int value;
        private Span labelSpan;
        private Span valueSpan;

        public ScoreLabel()
        {
            labelSpan = new Span
            {
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                ForegroundColor = Color.FromHex("#FFBDBDBD")
            };

            valueSpan = new Span
            {
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                FontFamily = Device.OnPlatform("MarkerFelt-Thin", "Droid Sans Mono", "Consolas"),
                ForegroundColor = Color.White,
            };

            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    new Label
                    {
                        FormattedText = new FormattedString
                        {
                            Spans = { labelSpan, valueSpan }
                        }
                    }
                }
            };

            Text = "Score";
            Value = 0;
        }

        public string Text
        {
            get { return labelSpan.Text.Substring(0, labelSpan.Text.Length - 3); }
            set { labelSpan.Text = value + ": "; }
        }

        public int Value
        {
            get { return value; }
            set
            {
                this.value = value;
                valueSpan.Text = string.Format("{0:0000,000}", value);
            }
        }
    }
}
