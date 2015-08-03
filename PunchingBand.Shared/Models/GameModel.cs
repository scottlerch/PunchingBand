using Windows.UI.Xaml;
using Newtonsoft.Json;
using PunchingBand.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Storage;
using PunchingBand.Recognition;
using PunchingBand.Utilities;

namespace PunchingBand.Models
{
    public class GameModel : PersistentModelBase
    {
        private readonly HistoryModel historyModel;
        private readonly PunchingModel punchingModel;
        private readonly UserModel userModel;

        private readonly TimeSpan speedComboInterval = TimeSpan.FromMilliseconds(250);

        // Game setup
        private GameMode gameMode = GameMode.MiniGame;
        private TimeSpan duration;
        private FistSides fistSide = FistSides.Unknown;
        private bool vrEnabled = false;

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
        private Metric skinTemperature = new Metric();
        private Metric heartrate = new Metric();
        private double caloriesBurned;
        private double? caloriesBurnedStart;

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

            PunchType = "Punch Type";
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

        public event EventHandler GameEnded = delegate { };
        public event EventHandler GameAborted = delegate { }; 

        private void PunchingModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "FistSides":
                    FistSide = punchingModel.FistSides;
                    break;
                case "CalorieCount":
                    caloriesBurnedStart = caloriesBurnedStart ?? punchingModel.CalorieCount;
                    CaloriesBurned = punchingModel.CalorieCount - caloriesBurnedStart.Value;
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
                PunchType = e.PunchRecognition.PunchType.ToString().SplitCamelCase();
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
                switch (GameMode)
                {
                    case GameMode.MiniGame:
                        yield return TimeSpan.FromSeconds(30);
                        break;
                    case GameMode.FreeformWorkout:
                    case GameMode.GuidedWorkout:
                        yield return TimeSpan.FromMinutes(5);
                        yield return TimeSpan.FromMinutes(10);
                        yield return TimeSpan.FromMinutes(15);
                        yield return TimeSpan.FromMinutes(20);
                        yield return TimeSpan.FromMinutes(30);
                        yield return TimeSpan.FromMinutes(45);
                        yield return TimeSpan.FromMinutes(60);
                        break;
                }
            }
        }

        [JsonIgnore]
        public bool GameDurationsEnabled { get { return GameDurations.Count() > 1;  } }

        [JsonIgnore]
        public IEnumerable<GameMode> GameModes
        {
            get
            { 
                yield return GameMode.MiniGame; 
                yield return GameMode.FreeformWorkout;
                yield return GameMode.GuidedWorkout; 
            }
        }

        public bool VrEnabled
        {
            get { return vrEnabled; }
            set { Set("VrEnabled", ref vrEnabled, value); }
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
            set
            {
                if (Set("GameMode", ref gameMode, value))
                {
                    RaisePropertyChanged("GameDurationsEnabled");
                    RaisePropertyChanged("GameDurations");
                    Duration = GameDurations.First();

                    historyModel.GameMode = gameMode;
                }
            }
        }

        [JsonIgnore]
        public FistSides FistSide
        {
            get { return fistSide; }
            set { Set("FistSide", ref fistSide, value); }
        }

        [JsonIgnore]
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
        public double CaloriesBurned
        {
            get { return caloriesBurned; }
            private set { Set("CaloriesBurned", ref caloriesBurned, value); }
        }

        [JsonIgnore]
        public TimeSpan Time
        {
            get { return duration - timeLeft; }
        }

        [JsonIgnore]
        public TimeSpan TimeLeft
        {
            get { return timeLeft; }
            private set
            {
                if (Set("TimeLeft", ref timeLeft, value))
                {
                    RaisePropertyChanged("Time");
                }
            }
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
            caloriesBurnedStart = null;

            TimeLeft = Duration;
            TimeLeftSeconds = (int)TimeLeft.TotalSeconds;
            Running = true;
            Score = 0;
            PunchCount = 0;
            SpeedComboCount = 0;
            PowerComboCount = 0;
            CaloriesBurned = 0;

            NewHighScore = false;

            punchStrength = new Metric(0.001);
            skinTemperature = new Metric();
            heartrate = new Metric();

            RaisePropertyChanged("PunchStrength");
            RaisePropertyChanged("PunchStrengthMeter"); 

            gameStartTime = DateTime.UtcNow;
        }

        internal void AbortGame()
        {
            GameAborted(this, EventArgs.Empty);
            Running = false;

            // Record workout sessions even if ended early
            if (gameMode == GameMode.FreeformWorkout || gameMode == GameMode.GuidedWorkout)
            {
                RecordGameInHistory();
            }
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
            NewHighScore = historyModel.SortedFilteredRecords.All(h => h.Score < Score);

            TimeLeft = TimeSpan.Zero;
            TimeLeftSeconds = 0;
            Running = false;

            GameEnded(this, EventArgs.Empty);

            RecordGameInHistory();
        }

        private void RecordGameInHistory()
        {
            historyModel.Records.Add(new HistoryInfo
            {
                Name = userModel.Name,
                Timestamp = DateTime.UtcNow,
                PunchStrenth = punchStrength,
                CaloriesBurned = caloriesBurned,
                SkinTemperature = skinTemperature,
                Heartrate = heartrate,
                Duration = Duration - TimeLeft,
                Score = Score,
                FistSide = FistSide,
                GameMode = gameMode,
                PunchCount = punchCount,
            });
        }
    }
}
