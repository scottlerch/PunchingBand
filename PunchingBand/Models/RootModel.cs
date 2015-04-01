using System;

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
            historyModel = new HistoryModel();
            userModel = new UserModel();
            punchingModel = new PunchingModel();
            gameModel = new GameModel(punchingModel, historyModel);
        }

        public RootModel(Action<Action> invokeOnUiThread)
        {
            historyModel = new HistoryModel();
            userModel = new UserModel();
            punchingModel = new PunchingModel(userModel, invokeOnUiThread);
            gameModel = new GameModel(punchingModel, historyModel);
        }
    }
}
