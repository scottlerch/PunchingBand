using Xamarin.Forms;

namespace PunchingBand.Pages.Views
{
    public class HistoryListView : ContentView
    {
        public HistoryListView()
        {
            var header = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    new Label
                    {
                        Text = "Name",
                    },
                    new Label
                    {
                        Text = "Score",
                    },
                    new Label
                    {
                        Text = "Count",
                    },
                    new Label
                    {
                        Text = "Strength",
                    },
                }
            };

            var listView = new ListView
            {
                ItemTemplate = new DataTemplate(() =>
                {
                    var nameLabel = new Label { FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
                    nameLabel.SetBinding(Label.TextProperty, "Name");

                    var scoreLabel = new Label { FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
                    scoreLabel.SetBinding(Label.TextProperty, "Score");

                    var countLabel = new Label { FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
                    countLabel.SetBinding(Label.TextProperty, "PunchCount");

                    var strengthLabel = new Label { FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
                    strengthLabel.SetBinding(Label.TextProperty, new Binding("PunchStrenth", BindingMode.OneWay, stringFormat: "{0}"));

                    return new ViewCell
                    {
                        View = new StackLayout
                        {
                            Padding = new Thickness(0, 5),
                            Orientation = StackOrientation.Horizontal,
                            Children =
                            {
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Vertical,
                                    VerticalOptions = LayoutOptions.Center,
                                    Spacing = 0,
                                    Children =
                                    {
                                        nameLabel,
                                        scoreLabel,
                                        countLabel,
                                        strengthLabel,
                                    }
                                }
                            }
                        }
                    };
                })
            };
            listView.SetBinding(ListView.ItemsSourceProperty, "SortedFilteredRecords");

            Content = new StackLayout
            {
                Children =
                {
                    header,
                    listView
                }
            };
        }
    }
}
