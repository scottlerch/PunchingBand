using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PunchingBand.Models
{
    public abstract class PersistentModelBase : ModelBase
    {
        private const string RootFolder = "UserData";
        private readonly JsonSerializer serializer = new JsonSerializer();

        private readonly Func<string, Task<Stream>> getReadStream;
        private readonly Func<string, Task<Stream>> getWriteStream;

        protected bool IsLoading { get; private set; }

        public PersistentModelBase(Func<string, Task<Stream>> getReadStream, Func<string, Task<Stream>> getWriteStream)
        {
            this.getReadStream = getReadStream;
            this.getWriteStream = getWriteStream;
        }

        public async Task Save()
        {
            if (IsLoading)
            {
                return;
            }

            try
            {
                using (var fileStream = await getWriteStream(Path.Combine(RootFolder, GetFileName())))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    serializer.Serialize(streamWriter, this);
                }
            }
            catch //(Exception ex)
            {
                // TODO: log
            }
        }

        public async Task Load()
        {
            try
            {
                IsLoading = true;

                using (var fileStream = await getReadStream(Path.Combine(RootFolder, GetFileName())))
                using (var streamReader = new StreamReader(fileStream))
                {
                    serializer.Populate(streamReader, this);
                }
            }
            catch (FileNotFoundException)
            {
                // Do nothing
            }
            catch (Exception)
            {
                // TODO: log
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetFileName()
        {
            return GetType().FullName + ".json";
        }
    }
}
