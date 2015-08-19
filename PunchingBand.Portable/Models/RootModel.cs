//#define MOCK_HISTORY
using System;
using System.Threading.Tasks;
using Accord;
using PunchingBand.Infrastructure;
using Microsoft.Band.Portable;
using System.IO;

namespace PunchingBand.Models
{
    public class RootModel : ModelBase
    {
        private readonly GameModel gameModel;
        private readonly PunchingModel punchingModel;
        private readonly UserModel userModel;
        private readonly HistoryModel historyModel;

        private readonly Func<string, Task<BandImage>> loadIcon;
        private readonly Func<string, Task<Stream>> getReadStream;
        private readonly Func<string, Task<Stream>> getWriteStream;

        public GameModel GameModel { get { return gameModel; } }

        public PunchingModel PunchingModel { get { return punchingModel; } }

        public UserModel UserModel { get { return userModel; } }

        public HistoryModel HistoryModel { get { return historyModel; } }

        public RootModel()
        {
            if (!PortableDesignMode.DesignModeEnabled)
            {
                throw new InvalidOperationException("Parameterless constructor can only be called by designer");
            }

            historyModel = new HistoryModel();
            userModel = new UserModel();
            punchingModel = new PunchingModel();
            gameModel = new GameModel(punchingModel, historyModel, userModel, null, null);
        }

        public RootModel(Action<Action> invokeOnUiThread, Func<string,Task<BandImage>> loadIcon, Func<string, Task<Stream>> getReadStream, Func<string, Task<Stream>> getWriteStream)
        {
            historyModel = new HistoryModel(getReadStream, getWriteStream);
            userModel = new UserModel(getReadStream, getWriteStream);
            punchingModel = new PunchingModel(userModel, invokeOnUiThread, loadIcon, getReadStream, getWriteStream);
            gameModel = new GameModel(punchingModel, historyModel, userModel, getReadStream, getWriteStream);

            this.loadIcon = loadIcon;
        }

        public async Task Load()
        {
#if MOCK_HISTORY
            var rand = new Random();
            var names = new[] {"Clippy", "Bob", "Cortana"};
            var workoutDurations = new[]
            {
                TimeSpan.FromMinutes(5), 
                TimeSpan.FromMinutes(15),
                TimeSpan.FromMinutes(20),
                TimeSpan.FromMinutes(30),
                TimeSpan.FromMinutes(30),
            };
            for (int i = 0; i < 50; i++)
            {
                var gameMode = (GameMode) rand.Next(3);
                var heartrate = new Metric();
                for (int j = 0; j < 10; j++)
                {
                    heartrate.Update(rand.Next(50, 200));
                }
                var strength = new Metric();
                for (int j = 0; j < 10; j++)
                {
                    strength.Update(rand.NextDouble() * 7.9 + 0.1);
                }
                historyModel.Records.Add(new HistoryInfo
                {
                    GameMode = gameMode,
                    Timestamp = DateTime.UtcNow.AddDays(-1*rand.Next(365)),
                    Duration = gameMode == GameMode.MiniGame ? TimeSpan.FromSeconds(30) : workoutDurations[rand.Next(workoutDurations.Length)],
                    Name = names[rand.Next(names.Length)],
                    Score = rand.Next(10, 1000),
                    CaloriesBurned = rand.Next(1000),
                    FistSide = FistSides.Right,
                    Heartrate = heartrate,
                    PunchCount = rand.Next(5, 100),
                    SkinTemperature = new Metric(96),
                    PunchStrenth = strength,
                });
            }
#else
            await historyModel.Load();
#endif

            await userModel.Load();
            await gameModel.Load();
        }
    }
}
