using PunchingBand.Pages.BindingConverters;
using PunchingBand.Pages.Views.Common;
using Xamarin.Forms;

namespace PunchingBand.Pages.Views
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

            var gender = new BindablePicker
            { 
            };
            gender.SetBinding(BindablePicker.ItemsSourceProperty, new Binding("Genders", BindingMode.OneWay, new EnumerableEnumTypeConverter()));
            gender.SetBinding(BindablePicker.SelectedItemProperty, new Binding("Gender", BindingMode.TwoWay, new EnumTypeConverter()));

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    new LabeledView
                    {
                        Text = "Name",
                        View = userName,
                    },
                    new LabeledView
                    {
                        Text = "Gender",
                        View = gender,
                    },
                    new LabeledView
                    {
                        Text = "Birth Date",
                        View = birthdate,
                    },
                    new LabeledView
                    {
                        Text = "Weight",
                        View = weight,
                    },
                }
            };
        }
    }
}
