using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using PunchingBand.Infrastructure;

namespace PunchingBand.Models
{
    public class GameModel : ModelBase
    {
        private readonly HistoryModel historyModel;
        private readonly PunchingModel punchingModel;

        // Game setup
        private GameMode gameMode = GameMode.TimeTrial;
        private TimeSpan duration;
        private FistSide fistSide;

        // Game state
        private int punchCount;
        private int score;
        private TimeSpan timeLeft;
        private int timeLeftSeconds;
        private bool running;

        private Metric punchStrength;
        private Metric caloriesBurned;
        private Metric skinTemperature;

        private StorageFile song;

        public GameModel()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                throw new InvalidOperationException("Parameterless constructor can only be called by designer");
            }

            duration = GameDurations.First();
            timeLeft = duration;
            timeLeftSeconds = (int)duration.TotalSeconds;
        }

        public GameModel(PunchingModel punchingModel, HistoryModel historyModel)
        {
            duration = GameDurations.First();
            timeLeft = duration;
            timeLeftSeconds = (int)duration.TotalSeconds;

            punchingModel.PunchStarted += PunchingModelOnPunchStarted;
            punchingModel.PunchEnded += PunchingModelOnPunchEnded;

            this.punchingModel = punchingModel;
            this.historyModel = historyModel;
        }

        private void PunchingModelOnPunchEnded(object sender, PunchEventArgs punchEventArgs)
        {
            if (Running)
            {
                PunchCount++;
                Score += (int) Math.Round(100.0*punchEventArgs.Strength);

                punchStrength.Update(punchEventArgs.Strength);
                RaisePropertyChanged("PunchStrength");
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

        public StorageFile Song
        {
            get { return song; }
            set { Set("Song", ref song, value); }
        }

        public GameMode GameMode
        {
            get { return gameMode; }
            set { Set("GameMode", ref gameMode, value); }
        }

        public FistSide FistSide
        {
            get { return fistSide; }
            set { Set("FistSide", ref fistSide, value); }
        }

        public TimeSpan Duration
        {
            get { return duration; }
            set { Set("Duration", ref duration, value); }
        }

        public double PunchStrength
        {
            get { return punchStrength.Last; }
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

            punchStrength = new Metric(0.001);
            caloriesBurned = new Metric();
            skinTemperature = new Metric();

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
                    EndGame();
                }
                else
                {
                    TimeLeft = diff;
                    TimeLeftSeconds = (int)Math.Ceiling(diff.TotalSeconds);
                    
                    caloriesBurned.Update(punchingModel.CalorieCount);

                    if (punchingModel.SkinTemperature.HasValue)
                    {
                        skinTemperature.Update(punchingModel.SkinTemperature.Value);
                    }
                }
            }
        }

        private void EndGame()
        {
            TimeLeft = TimeSpan.Zero;
            TimeLeftSeconds = 0;
            Running = false;

            historyModel.Records.Add(new HistoryInfo
            {
                Timestamp = DateTime.UtcNow,
                PunchStrenth = punchStrength,
                CaloriesBurned = caloriesBurned,
                SkinTemperature = skinTemperature,
                Duration = Duration, 
                Score = Score,
                FistSide = FistSide,
            });
        }
    }
}
