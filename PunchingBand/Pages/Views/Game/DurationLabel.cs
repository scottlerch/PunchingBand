﻿using System;
using Xamarin.Forms;

namespace PunchingBand.Pages.Views.Game
{
    public class DurationLabel : ContentView
    {
        private TimeSpan value;
        private Span labelSpan;
        private Span valueSpan;
        private Label durationLabel;

        public DurationLabel()
        {
            labelSpan = new Span
            {
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                ForegroundColor = Color.FromHex("#FF909090")
            };

            valueSpan = new Span
            {
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                ForegroundColor = Color.White,
            };

            durationLabel = new Label
            {
                FormattedText = new FormattedString
                {
                    Spans = { valueSpan, labelSpan }
                }
            };

            Content = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    durationLabel
                }
            };

            Text = " sec.";
            Value = TimeSpan.Zero;
        }

        public string Text
        {
            get { return labelSpan.Text.Substring(0, labelSpan.Text.Length); }
            set { labelSpan.Text = value; }
        }

        public TimeSpan Value
        {
            get { return value; }
            set
            {
                this.value = value;
                valueSpan.Text = string.Format("{0:ss}.{0:ff}", value);
            }
        }
    }
}
