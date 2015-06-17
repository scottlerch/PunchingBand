using Newtonsoft.Json;
using PunchingBand.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Storage;
using PunchingBand.Models.Enums;

namespace PunchingBand.Models
{
    public class GameModel : PersistentModelBase
    {
        private readonly HistoryModel historyModel;
        private readonly PunchingModel punchingModel;

        private readonly TimeSpan speedComboInterval = TimeSpan.FromMilliseconds(250);

        // Game setup
        private GameMode gameMode = GameMode.TimeTrial;
        private TimeSpan duration;
        private FistSides fistSide = FistSides.Right;

        // Game state
        private int punchCount;
        private int score;
        private TimeSpan timeLeft;
        private int timeLeftSeconds;
        private bool running;

        // Combo trackers
        private int speedComboCount;
        private int powerComboCount;
        private DateTime lastPunchTime = DateTime.MinValue;
        private double lastPunchStrength = 0.0;
        //private double currentPunchStrength = 0.0;

        // Game performance metrics
        private Metric punchStrength;
        private Metric caloriesBurned;
        private Metric skinTemperature;

        private StorageFile song;

        public GameModel()
        {
            if (!DesignMode.DesignModeEnabled)
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
            punchingModel.Punching += PunchingModelOnPunching;
            punchingModel.PropertyChanged += PunchingModelOnPropertyChanged;

            this.punchingModel = punchingModel;
            this.historyModel = historyModel;

            this.PropertyChanged += GameModelOnPropertyChanged;
        }

        private void PunchingModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "FistSides":
                    // TODO: what if left and right sides?
                    FistSide = punchingModel.FistSides;
                    break;
            }
        }

        private async void GameModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "GameMode":
                case "Duration":
                case "FistSide":
                    await Save();
                    break;
            }
        }

        private void PunchingModelOnPunching(object sender, PunchEventArgs e)
        {
            //currentPunchStrength = e.Strength;
            //RaisePropertyChanged("PunchStrengthMeter");
        }

        private void PunchingModelOnPunchEnded(object sender, PunchEventArgs punchEventArgs)
        {
            if (Running)
            {
                PunchCount++;
                UpdateCombos(punchEventArgs);

                var points = 100.0 * punchEventArgs.Strength;

                if (powerComboCount > 1)
                {
                    points *= 2;
                }

                if (speedComboCount > 1)
                {
                    points *= 2;
                }

                Score += (int) Math.Round(points);

                punchStrength.Update(punchEventArgs.Strength);
                RaisePropertyChanged("PunchStrength");
                RaisePropertyChanged("PunchStrengthMeter"); 
            }
        }

        private void UpdateCombos(PunchEventArgs punchEventArgs)
        {
            var punchTime = DateTime.UtcNow;

            if (punchTime - lastPunchTime < speedComboInterval)
            {
                SpeedComboCount++;
            }
            else
            {
                SpeedComboCount = 0;
            }

            if (1.0 - punchEventArgs.Strength < 0.001 && 1.0 - punchStrength.Last < 0.001)
            {
                PowerComboCount++;
            }
            else
            {
                PowerComboCount = 0;
            }

            lastPunchTime = punchTime;
        }

        private void PunchingModelOnPunchStarted(object sender, PunchEventArgs punchEventArgs)
        {
            
        }

        [JsonIgnore]
        public IEnumerable<TimeSpan> GameDurations
        {
            get
            {
                yield return TimeSpan.FromSeconds(15);
                yield return TimeSpan.FromSeconds(30);
                yield return TimeSpan.FromSeconds(60);
            }
        }

        [JsonIgnore]
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

        public FistSides FistSide
        {
            get { return fistSide; }
            set { Set("FistSide", ref fistSide, value); }
        }

        public TimeSpan Duration
        {
            get { return duration; }
            set { Set("Duration", ref duration, value); }
        }

        [JsonIgnore]
        public double PunchStrength
        {
            get { return punchStrength.Last; }
        }

        [JsonIgnore]
        public double PunchStrengthMeter
        {
            get { return 1.0 - punchStrength.Last; }
        }

        [JsonIgnore]
        public bool Running
        {
            get { return running; }
            private set { Set("Running", ref running, value); }
        }

        [JsonIgnore]
        public int Score
        {
            get { return score; }
            private set { Set("Score", ref score, value); }
        }

        [JsonIgnore]
        public TimeSpan TimeLeft
        {
            get { return timeLeft; }
            private set { Set("TimeLeft", ref timeLeft, value); }
        }

        [JsonIgnore]
        public int TimeLeftSeconds
        {
            get { return timeLeftSeconds; }
            private set { Set("TimeLeftSeconds", ref timeLeftSeconds, value); }
        }

        [JsonIgnore]
        public int PunchCount
        {
            get { return punchCount; }
            private set { Set("PunchCount", ref punchCount, value); }
        }

        [JsonIgnore]
        public int SpeedComboCount
        {
            get { return speedComboCount; }
            private set
            {
                Set("SpeedComboCount", ref speedComboCount, value);
                RaisePropertyChanged("SpeedComboText");
            }
        }

        [JsonIgnore]
        public string SpeedComboText
        {
            get
            {
                if (speedComboCount == 1 || DesignMode.DesignModeEnabled)
                {
                    return "speed";
                }

                if (speedComboCount == 0)
                {
                    return string.Empty;
                }

                return speedComboCount.ToString() + "x";
            }
        }

        [JsonIgnore]
        public int PowerComboCount
        {
            get { return powerComboCount; }
            private set
            {
                Set("powerComboCount", ref powerComboCount, value);
                RaisePropertyChanged("PowerComboText");
            }
        }

        [JsonIgnore]
        public string PowerComboText
        {
            get
            {
                if (powerComboCount == 1 || DesignMode.DesignModeEnabled)
                {
                    return "power";
                }

                if (powerComboCount == 0)
                {
                    return string.Empty;
                }

                return powerComboCount.ToString() + "x";
            }
        }

        internal void StartGame()
        {
            TimeLeft = Duration;
            TimeLeftSeconds = (int)TimeLeft.TotalSeconds;
            Running = true;
            Score = 0;
            PunchCount = 0;
            SpeedComboCount = 0;
            PowerComboCount = 0;

            punchStrength = new Metric(0.001);
            caloriesBurned = new Metric();
            skinTemperature = new Metric();

            RaisePropertyChanged("PunchStrength");
            RaisePropertyChanged("PunchStrengthMeter"); 

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
                GameMode = gameMode,
                PunchCount = punchCount,
            });
        }
    }
}
