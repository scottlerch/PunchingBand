using PunchingBand.Pages.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;

namespace PunchingBand.Pages
{
    public class MiniGamePage : ContentPage
    {
        public MiniGamePage()
        {
            var absoluteLayout = new AbsoluteLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            
            absoluteLayout.Children.Add(new PunchIndicator
            {
                WidthRequest = 200,
                HeightRequest = 200,
            });

            Content = absoluteLayout;
        }
    }
}
