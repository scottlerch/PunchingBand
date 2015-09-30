using PunchingBand.Models;
using PunchingBand.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PunchingBand
{
    public class XamarinApp : Application
    {
        public static RootModel RootModel { get; private set; }

        public XamarinApp()
        {
            Func<string, Task<Stream>> readStream = fileName => Task.FromResult(Stream.Null);
            Func<string, Task<Stream>> writeStream = fileName => Task.FromResult(Stream.Null);

            RootModel = new RootModel(Device.BeginInvokeOnMainThread, readStream, writeStream);
            RootModel.Load();

            MainPage = new HomePage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
