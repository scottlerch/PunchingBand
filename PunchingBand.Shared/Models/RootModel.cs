//#define MOCK_HISTORY
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Accord;
using PunchingBand.Infrastructure;

namespace PunchingBand.Models
{
    public class RootModel : ModelBase
    {
        private readonly GameModel gameModel;
        private readonly PunchingModel punchingModel;
        private readonly UserModel userModel;
        private readonly HistoryModel historyModel;

        public GameModel GameModel { get { return gameModel; } }

        public PunchingModel PunchingModel { get { return punchingModel; } }

        public UserModel UserModel { get { return userModel; } }

        public HistoryModel HistoryModel { get { return historyModel; } }

        public RootModel()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                throw new InvalidOperationException("Parameterless constructor can only be called by designer");
            }

            historyModel = new HistoryModel();
            userModel = new UserModel();
            punchingModel = new PunchingModel();
            gameModel = new GameModel(punchingModel, historyModel, userModel);
        }

        public RootModel(Action<Action> invokeOnUiThread)
        {
            historyModel = new HistoryModel();
            userModel = new UserModel();
            punchingModel = new PunchingModel(userModel, invokeOnUiThread);
            gameModel = new GameModel(punchingModel, historyModel, userModel);
        }

        public async Task Load()
        {
#if MOCK_HISTORY
            var rand = new Random();
            for (int i = 0; i < 25; i++)
            {
                var gameMode = (GameMode) rand.Next(3);
                historyModel.Records.Add(new HistoryInfo
                {
                    GameMode = gameMode,
                    Timestamp = DateTime.UtcNow.AddMinutes(-1*rand.Next(100000)),
                    Duration = gameMode == GameMode.MiniGame ? TimeSpan.FromSeconds(15) : TimeSpan.FromMinutes(5),
                    Name = gameMode.ToString(),
                    Score = rand.Next(10, 10000),
                    CaloriesBurned = rand.Next(1),
                    FistSide = FistSides.Right,
                    Heartrate = new Metric(rand.Next(50, 190)),
                    PunchCount = rand.Next(5, 100), PunchStrenth = new Metric(rand.NextDouble() * 8.0),
                    SkinTemperature = new Metric(96),
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
