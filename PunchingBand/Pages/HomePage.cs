using PunchingBand.Pages.Views;
using Xamarin.Forms;

namespace PunchingBand.Pages
{
    public class HomePage : TabbedPage
    {
        public HomePage()
        {
            Children.Add(
                new ContentPage
                {
                    Title = "setup",
                    Content = new GameEditor
                    {
                        BindingContext = XamarinApp.RootModel.GameModel,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                    }
                });

            Children.Add(
                new ContentPage
                {
                    Title = "user",
                    Content = new UserEditor
                    {
                        BindingContext = XamarinApp.RootModel.UserModel,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                    }
                });

            Children.Add(
                new ContentPage
                {
                    Title = "history",
                    Content = new HistoryListView
                    {
                        BindingContext = XamarinApp.RootModel.HistoryModel,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                    }
                });
        }
    }
}
