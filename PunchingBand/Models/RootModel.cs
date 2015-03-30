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

        public GameModel GameModel { get { return gameModel; } }

        public PunchingModel PunchingModel { get { return punchingModel; } }

        public RootModel()
        {
            punchingModel = new PunchingModel();
            gameModel = new GameModel(punchingModel);
        }

        public RootModel(Action<Action> invokeOnUiThread)
        {
            punchingModel = new PunchingModel(invokeOnUiThread);
            gameModel = new GameModel(punchingModel);
        }
    }
}
