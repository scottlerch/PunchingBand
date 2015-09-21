using System;
using Xamarin.Forms;

namespace PunchingBand.Pages.Controls
{
    public class TemperatureLabel : ContentView
    {
        private double value;
        private Label label;

        public TemperatureLabel()
        {
            label = new Label
            {
                FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                TextColor = Color.FromHex("#FF646464")
            };

            Content = label;

            Value = 98.6;
        }

        public double Value
        {
            get { return value; }
            set
            {
                this.value = value;
                label.Text = string.Format("{0:0.0}°", value);
            }
        }
    }
}
