using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace PunchingBand.Models
{
    public abstract class PersistentModelBase : ModelBase
    {
        private const string RootFolder = "UserData";
        private readonly JsonSerializer serializer = new JsonSerializer();

        public async Task Save()
        {
            var dataFolder = await GetFolder();
            var file = await dataFolder.CreateFileAsync(GetFileName(), CreationCollisionOption.ReplaceExisting);

            using (var fileStream = await file.OpenStreamForWriteAsync())
            using (var streamWriter = new StreamWriter(fileStream))
            {
                serializer.Serialize(streamWriter, this);
            }
        }

        public async Task Load()
        {
            var dataFolder = await GetFolder();

            try
            {
                var file = await dataFolder.GetFileAsync(GetFileName());

                using (var fileStream = await file.OpenStreamForReadAsync())
                using (var streamReader = new StreamReader(fileStream))
                {
                    serializer.Populate(streamReader, this);
                }
            }
            catch (FileNotFoundException)
            {
                // Do nothing
            }
        }

        private async Task<StorageFolder> GetFolder()
        {
            var local = ApplicationData.Current.LocalFolder;
            return await local.CreateFolderAsync(RootFolder, CreationCollisionOption.OpenIfExists);
        }

        private string GetFileName()
        {
            return GetType().FullName + ".json";
        }
    }
}
