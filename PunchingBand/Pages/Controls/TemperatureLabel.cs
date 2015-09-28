using System;
using Xamarin.Forms;

namespace PunchingBand.Pages.Controls
{
    public class TemperatureLabel : ContentView
    {
        private double? value;
        private Label label;

        public TemperatureLabel()
        {
            label = new Label
            {
                FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                TextColor = Color.FromHex("#FF646464")
            };

            Content = label;

            Value = null;
        }

        public double? Value
        {
            get { return value; }
            set
            {
                this.value = value;
                label.Text = value == null ? "N/A" : string.Format("{0:0.0}°", value);
            }
        }
    }
}
