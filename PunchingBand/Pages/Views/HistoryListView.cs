using Xamarin.Forms;

namespace PunchingBand.Pages.Views
{
    public class HistoryListView : ContentView
    {
        public HistoryListView()
        {
            var header = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition
                    {
                        Height = GridLength.Auto,
                    }
                },
                Children =
                {
                    new Label
                    {
                        Text = "Name",
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        TextColor = Color.FromHex("#FF909090"),
                    },
                    new Label
                    {
                        Text = "Score",
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        TextColor = Color.FromHex("#FF909090"),
                    },
                    new Label
                    {
                        Text = "Count",
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        TextColor = Color.FromHex("#FF909090"),
                    },
                    new Label
                    {
                        Text = "Strength",
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        TextColor = Color.FromHex("#FF909090"),
                    },
                },
            };

            for (var i = 0; i < header.Children.Count; i++)
            {
                Grid.SetColumn(header.Children[i], i);
            }

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

                    Grid.SetColumn(nameLabel, 0);
                    Grid.SetColumn(scoreLabel, 1);
                    Grid.SetColumn(countLabel, 2);
                    Grid.SetColumn(strengthLabel, 3);

                    return new ViewCell
                    {
                        View = new Grid
                        {
                            RowDefinitions =
                            {
                                new RowDefinition
                                {
                                    Height = GridLength.Auto,
                                }
                            },
                            Children =
                            {
                                nameLabel,
                                scoreLabel,
                                countLabel,
                                strengthLabel,
                            }
                        }
                    };
                })
            };
            listView.SetBinding(ListView.ItemsSourceProperty, "SortedFilteredRecords");

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    header,
                    listView, 
                }
            };
        }
    }
}
