using Newtonsoft.Json;
using PunchingBand.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Storage;
using PunchingBand.Recognition;

namespace PunchingBand.Models
{
    public class GameModel : PersistentModelBase
    {
        private readonly HistoryModel historyModel;
        private readonly PunchingModel punchingModel;
        private readonly UserModel userModel;

        private readonly TimeSpan speedComboInterval = TimeSpan.FromMilliseconds(250);

        // Game setup
        private GameMode gameMode = GameMode.TimeTrial;
        private TimeSpan duration;
        private FistSides fistSide = FistSides.Unknown;

        // Game state
        private int punchCount;
        private int score;
        private TimeSpan timeLeft;
        private int timeLeftSeconds;
        private bool running;
        private string punchType;
        private bool newHighScore;

        // Combo trackers
        private int speedComboCount;
        private int powerComboCount;
        private DateTime lastPunchTime = DateTime.MinValue;
        private double lastPunchStrength = 0.0;
        //private double currentPunchStrength = 0.0;

        // Game performance metrics
        private Metric punchStrength = new Metric();
        private Metric caloriesBurned = new Metric();
        private Metric skinTemperature = new Metric();
        private Metric heartrate = new Metric();

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

        public GameModel(PunchingModel punchingModel, HistoryModel historyModel, UserModel userModel)
        {
            duration = GameDurations.First();
            timeLeft = duration;
            timeLeftSeconds = (int)duration.TotalSeconds;

            punchingModel.PunchStarted += PunchingModelOnPunchStarted;
            punchingModel.PunchEnded += PunchingModelOnPunchEnded;
            punchingModel.Punching += PunchingModelOnPunching;
            punchingModel.PunchRecognized += PunchingModelOnPunchRecognized;
            punchingModel.PropertyChanged += PunchingModelOnPropertyChanged;

            this.punchingModel = punchingModel;
            this.historyModel = historyModel;
            this.userModel = userModel;

            this.PropertyChanged += GameModelOnPropertyChanged;
        }

        private void PunchingModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "FistSides":
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

        private void PunchingModelOnPunchRecognized(object sender, PunchEventArgs e)
        {
            if (e.PunchRecognition != PunchRecognition.Unknown && Running)
            {
                PunchType = e.PunchRecognition.PunchType.ToString();
                RaisePropertyChanged("PunchType");
            }
            else
            {
                PunchType = string.Empty;
            }
        }

        private void PunchingModelOnPunchEnded(object sender, PunchEventArgs punchEventArgs)
        {
            if (Running && punchEventArgs.Strength.HasValue)
            {
                PunchCount++;
                UpdateCombos(punchEventArgs);

                var points = 100.0 * punchEventArgs.Strength.Value;

                if (powerComboCount > 1)
                {
                    points *= 2;
                }

                if (speedComboCount > 1)
                {
                    points *= 2;
                }

                Score += (int) Math.Round(points);

                punchStrength.Update(punchEventArgs.Strength.Value);
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

            if (PunchDetector.MaximumAcceleration - punchEventArgs.Strength < 0.001 && 
                PunchDetector.MaximumAcceleration - punchStrength.Last < 0.001)
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
        public bool NewHighScore
        {
            get { return newHighScore; }
            set { Set("NewHighScore", ref newHighScore, value); }
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

        [JsonIgnore]
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
            get { return 1.0 - (punchStrength.Last / PunchDetector.MaximumAcceleration); }
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
        public string PunchType
        {
            get { return punchType; }
            private set { Set("PunchType", ref punchType, value); }
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

            NewHighScore = false;

            punchStrength = new Metric(0.001);
            caloriesBurned = new Metric();
            skinTemperature = new Metric();
            heartrate = new Metric();

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

                    if (punchingModel.HeartRate.HasValue)
                    {
                        heartrate.Update(punchingModel.HeartRate.Value);
                    }
                }
            }
        }

        private void EndGame()
        {
            NewHighScore = historyModel.Records.All(h => h.Score < Score);

            TimeLeft = TimeSpan.Zero;
            TimeLeftSeconds = 0;
            Running = false;

            historyModel.Records.Add(new HistoryInfo
            {
                Name = userModel.Name,
                Timestamp = DateTime.UtcNow,
                PunchStrenth = punchStrength,
                CaloriesBurned = caloriesBurned,
                SkinTemperature = skinTemperature,
                Heartrate = heartrate,
                Duration = Duration, 
                Score = Score,
                FistSide = FistSide,
                GameMode = gameMode,
                PunchCount = punchCount,
            });
        }
    }
}
