using PunchingBand.Pages.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace PunchingBand.Pages
{
    public class HomePageFactory
    {
        public Page CreateCarouselPage()
        {
            var page = new CarouselPage();

            InitiliazePage(page);

            foreach (var pair in CreateViews(page))
            {
                page.Children.Add(
                    new ContentPage
                    {
                        Title = pair.Key,
                        Content = pair.Value,
                    });
            }

            return page;
        }

        public Page CreateTabbedPage()
        {
            var page = new TabbedPage();

            InitiliazePage(page);

            foreach (var pair in CreateViews(page))
            {
                page.Children.Add(
                    new ContentPage
                    {
                        Title = pair.Key,
                        Content = pair.Value,
                    });
            }

            return page;
        }

        public Page CreateContentPage()
        {
            var page = new ContentPage();

            InitiliazePage(page);

            var layout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };

            foreach (var pair in CreateViews(page))
            {
                pair.Value.VerticalOptions = LayoutOptions.Start;
                layout.Children.Add(new StackLayout
                {
                    Padding = new Thickness(10, 10, 10, 10),
                    Orientation = StackOrientation.Vertical,
                    Children =
                    {
                        new Label
                        {
                            Text = pair.Key,
                            TextColor = Color.White,
                            FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                        },
                        pair.Value,
                    }
                });
            }

            page.Content = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    new Frame
                    {
                        Padding = new Thickness(10, 2, 10, 2),
                        Content = new Label
                        {
                            Text = "PUNCHING BAND",
                            FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
                        },
                    },
                    new BoxView
                    {
                        HeightRequest = 30,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        BackgroundColor = Color.FromHex("#56318e"),
                    },
                    layout
                }
            };

            return page;
        }

        private void InitiliazePage(Page page)
        {
            page.BackgroundColor = Color.Black.AddLuminosity(0.1);
        }

        private View CreateHistoryView()
        {
            return new HistoryListView
            {
                BindingContext = XamarinApp.RootModel.HistoryModel,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
            };
        }

        private View CreateUserView()
        {
            return new UserEditor
            {
                BindingContext = XamarinApp.RootModel.UserModel,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
            };
        }

        private View CreateSetupView(Page page)
        {
            var button = new Button
            {
                Text = "FIGHT!",
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                TextColor = Color.White,
                BorderColor = Color.White,
            };

            button.Clicked += (sender, e) => page.Navigation.PushModalAsync(new MiniGamePage());

            return new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Children =
                {
                    new GameEditor
                    {
                        BindingContext = XamarinApp.RootModel.GameModel,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                    },
                    button
                }
            };
        }

        private Dictionary<string, View> CreateViews(Page page)
        {
            return new Dictionary<string, View>
            {
                { "setup", CreateSetupView(page) },
                { "user", CreateUserView() },
                { "history", CreateHistoryView() }
            };
        }
    }
}
