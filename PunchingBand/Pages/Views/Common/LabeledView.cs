using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PunchingBand.Pages.Views.Common
{
    public class LabeledView : ContentView
    {
        private Label label;
        private StackLayout stackLayout;
        private View view;
        private Spacer spacer;

        public LabeledView()
        {
            view = new Frame();
            spacer = new Spacer();
            label = new Label
            {
                Text = "Name",
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                TextColor = Color.FromHex("#FF909090"),
            };
            stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    label,
                    view,
                    spacer,
                }
            };

            Content = stackLayout;
        }

        public string Text
        {
            get { return label.Text; }
            set { label.Text = value; }
        }

        public View View
        {
            get { return view; }
            set
            {
                if (view != null)
                {
                    stackLayout.Children.Remove(view);
                }
                view = value;
                stackLayout.Children.Insert(1, view);
            }
        }
    }
}
