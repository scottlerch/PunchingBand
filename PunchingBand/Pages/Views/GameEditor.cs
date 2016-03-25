using PunchingBand.Pages.BindingConverters;
using PunchingBand.Pages.Views.Common;
using Xamarin.Forms;

namespace PunchingBand.Pages.Views
{
    public class GameEditor : ContentView
    {
        public GameEditor()
        {
            var gameMode = new BindablePicker
            {
            };
            gameMode.SetBinding(BindablePicker.ItemsSourceProperty, new Binding("GameModes", BindingMode.OneWay, new EnumerableEnumTypeConverter()));
            gameMode.SetBinding(BindablePicker.SelectedItemProperty, new Binding("GameMode", BindingMode.TwoWay, new EnumTypeConverter()));

            var duration = new BindablePicker
            {
            };

            duration.SetBinding(BindablePicker.ItemsSourceProperty, new Binding("GameDurations", BindingMode.OneWay, new EnumerableTimeSpanTypeConverter()));
            duration.SetBinding(BindablePicker.SelectedItemProperty, new Binding("Duration", BindingMode.TwoWay, new TimeSpanTypeConverter()));

            var labeledDuration = new LabeledView
            {
                Text = "Duration",
                View = duration
            };
            labeledDuration.SetBinding(LabeledView.IsVisibleProperty, new Binding("GameDurationsEnabled", BindingMode.OneWay));

            // TODO: add music selection

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    new LabeledView
                    {
                        Text = "Mode",
                        View = gameMode
                    },
                    labeledDuration,
                }
            };
        }
    }
}
