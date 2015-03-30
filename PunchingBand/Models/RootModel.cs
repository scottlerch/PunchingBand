using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PunchingBand.Models
{
    public class RootModel : ModelBase
    {
        private readonly GameModel gameModel;
        private readonly PunchingModel punchingModel;
        private readonly UserModel userModel;

        public GameModel GameModel { get { return gameModel; } }

        public PunchingModel PunchingModel { get { return punchingModel; } }

        public UserModel UserModel { get { return userModel; } }

        public RootModel()
        {
            userModel = new UserModel();
            punchingModel = new PunchingModel();
            gameModel = new GameModel(punchingModel);
        }

        public RootModel(Action<Action> invokeOnUiThread)
        {
            userModel = new UserModel();
            punchingModel = new PunchingModel(userModel, invokeOnUiThread);
            gameModel = new GameModel(punchingModel);
        }
    }
}
