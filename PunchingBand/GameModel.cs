using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchingBand
{
    public class GameModel : ModelBase
    {
        private TimeSpan duration;

        public IEnumerable<TimeSpan> GameDurations
        {
            get
            {
                yield return TimeSpan.FromSeconds(15);
                yield return TimeSpan.FromSeconds(30);
                yield return TimeSpan.FromSeconds(60);
            }
        }

        public TimeSpan Duration
        {
            get { return duration; }
            set { Set("Duration", ref duration, value); }
        }

        public GameModel()
        {
            duration = GameDurations.First();
        }
    }
}
