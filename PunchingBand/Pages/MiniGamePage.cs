﻿using PunchingBand.Models;
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

            var userModel = new UserModel();
            var control = new UserEditor
            {
                WidthRequest = 350,
                HeightRequest = 350,
                BindingContext = userModel,
            };


            absoluteLayout.Children.Add(control);

            Content = absoluteLayout;

            //this.Appearing += async (sender, args) => await control.Begin();
        }

    }
}
