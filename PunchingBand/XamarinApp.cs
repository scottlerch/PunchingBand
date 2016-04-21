using PCLStorage;
using PunchingBand.Models;
using PunchingBand.Pages;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PunchingBand
{
    public class XamarinApp : Application
    {
        public static RootModel RootModel { get; private set; }

        public XamarinApp()
        {
            RootModel = new RootModel(Device.BeginInvokeOnMainThread, GetReadStream, GetWriteStream);

            var homePageFactory = new HomePageFactory();

            // TODO: Fix issues with Acr.DeviceInfo on Windows81 and UWP
            //if (Acr.DeviceInfo.DeviceInfo.Hardware.IsTablet)
            if (true)
            {
                MainPage = homePageFactory.CreateContentPage();
            }
            else
            {
                MainPage = homePageFactory.CreateTabbedPage();
            }
        }

        private static async Task<Stream> GetReadStream(string relativeFilePath)
        {
            var directory = Path.GetDirectoryName(relativeFilePath);
            var fileName = Path.GetFileName(relativeFilePath);
            var rootFolder = FileSystem.Current.LocalStorage;
            var folder = await rootFolder.CreateFolderAsync(directory, CreationCollisionOption.OpenIfExists);

            if (await folder.CheckExistsAsync(fileName) == ExistenceCheckResult.FileExists)
            {
                var file = await folder.GetFileAsync(fileName);
                return await file.OpenAsync(PCLStorage.FileAccess.Read);
            }

            throw new FileNotFoundException(string.Format("Unable to find file '{0}'", relativeFilePath));
        }

        private static async Task<Stream> GetWriteStream(string relativeFilePath)
        {
            var directory = Path.GetDirectoryName(relativeFilePath);
            var fileName = Path.GetFileName(relativeFilePath);
            var rootFolder = FileSystem.Current.LocalStorage;
            var folder = await rootFolder.CreateFolderAsync(directory, CreationCollisionOption.OpenIfExists);
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            return await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite);
        }

        protected override async void OnStart()
        {
            await RootModel.Load();

            // Handle when your app starts
            await RootModel.PunchingModel.Connect();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            RootModel.PunchingModel.Disconnect();
        }

        protected override async void OnResume()
        {
            // Handle when your app resumes
            await RootModel.PunchingModel.Connect();
        }
    }
}
