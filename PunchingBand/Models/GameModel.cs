using System;
using System.Collections.Generic;
using System.Linq;

namespace PunchingBand.Models
{
    public class GameModel : ModelBase
    {
        private TimeSpan duration;
        private int punchCount;
        private double punchStrength;
        private int score;
        private TimeSpan timeLeft;
        private int timeLeftSeconds;
        private bool running;

        public GameModel(PunchingModel punchingModel)
        {
            duration = GameDurations.First();
            timeLeft = duration;
            timeLeftSeconds = (int)duration.TotalSeconds;

            punchingModel.PunchStarted += PunchingModelOnPunchStarted;
            punchingModel.PunchEnded += PunchingModelOnPunchEnded;
        }

        private void PunchingModelOnPunchEnded(object sender, PunchEventArgs punchEventArgs)
        {
            if (Running)
            {
                PunchCount++;
                PunchStrength = punchEventArgs.Strength;
                Score += (int) Math.Round(100.0*punchEventArgs.Strength);
            }
        }

        private void PunchingModelOnPunchStarted(object sender, PunchEventArgs punchEventArgs)
        {
            
        }

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

        public double PunchStrength
        {
            get { return punchStrength; }
            set { Set("PunchStrength", ref punchStrength, value); }
        }

        public bool Running
        {
            get { return running; }
            set { Set("Running", ref running, value); }
        }

        public int Score
        {
            get { return score; }
            set { Set("Score", ref score, value); }
        }

        public TimeSpan TimeLeft
        {
            get { return timeLeft; }
            set { Set("TimeLeft", ref timeLeft, value); }
        }

        public int TimeLeftSeconds
        {
            get { return timeLeftSeconds; }
            set { Set("TimeLeftSeconds", ref timeLeftSeconds, value); }
        }

        public int PunchCount
        {
            get { return punchCount; }
            set { Set("PunchCount", ref punchCount, value); }
        }

        internal void StartGame()
        {
            TimeLeft = Duration;
            TimeLeftSeconds = (int)TimeLeft.TotalSeconds;
            Running = true;
            Score = 0;
            PunchCount = 0;
            PunchStrength = 0.001;
            gameStartTime = DateTime.UtcNow;
        }

        internal void StopGame()
        {
            Running = false;
        }

        private DateTime gameStartTime;

        public void Update()
        {
            if (Running)
            {
                var diff = Duration - (DateTime.UtcNow - gameStartTime);
                if (diff <= TimeSpan.Zero)
                {
                    TimeLeft = TimeSpan.Zero;
                    TimeLeftSeconds = 0;
                    Running = false;
                }
                else
                {
                    TimeLeft = diff;
                    TimeLeftSeconds = (int)Math.Ceiling(diff.TotalSeconds);
                }
            }
        }
    }
}
