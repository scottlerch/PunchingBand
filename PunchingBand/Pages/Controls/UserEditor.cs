using PunchingBand.Pages.BindingConverters;
using PunchingBand.Utilities;
using System;
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
            userName.SetBinding(Entry.TextProperty, new Binding("Name", BindingMode.OneWay));

            var weight = new Entry
            {
                Keyboard = Keyboard.Numeric,
                Placeholder = "Enter weight",
            };
            weight.SetBinding(Entry.TextProperty, new Binding("Weight", BindingMode.OneWay, new WeightConverter(), WeightUnit.Lbs.ToString()));
     
            var birthdate = new DatePicker
            {
            };
            birthdate.SetBinding(DatePicker.DateProperty, new Binding("BirthDateTime", BindingMode.OneWay));

            var gender = new Picker
            { 
                SelectedIndex = 0,
            };
            foreach (var value in (Gender[])Enum.GetValues(typeof(Gender)))
            {
                gender.Items.Add(value.ToString().SplitCamelCase());
            }
            gender.SetBinding(Picker.SelectedIndexProperty, new Binding("Gender", BindingMode.TwoWay, new EnumTypeConverter()));

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
