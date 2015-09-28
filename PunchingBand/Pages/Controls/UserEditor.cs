using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PunchingBand.Pages.Controls
{
    public class UserEditor : ContentView
    {
        public UserEditor()
        {
            var userName = new Entry
            {
                Keyboard = Keyboard.Text,
                Placeholder = "Enter name",
            };

            var weight = new Entry
            {
                Keyboard = Keyboard.Numeric,
                Placeholder = "Enter weight",
            };

            var birthdate = new DatePicker
            {
        
            };

            var gender = new Picker
            { 
                Items =
                {
                    "Male",
                    "Female",
                },
                SelectedIndex = 0,
            };

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    new Label { Text = "Name", FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)), TextColor = Color.FromHex("#FF909090") },
                    userName,
                    new Frame { Padding = new Thickness(0, 0, 0, 10) },
                    new Label { Text = "Gender", FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)), TextColor = Color.FromHex("#FF909090") },
                    gender,
                    new Frame { Padding = new Thickness(0, 0, 0, 10) },
                    new Label { Text = "Birth Date", FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)), TextColor = Color.FromHex("#FF909090") },
                    birthdate,
                    new Frame { Padding = new Thickness(0, 0, 0, 10) },
                    new Label { Text = "Weight", FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)), TextColor = Color.FromHex("#FF909090") },
                    weight,
                }
            };
        }
    }
}
