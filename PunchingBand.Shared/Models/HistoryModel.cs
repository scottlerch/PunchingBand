using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.ApplicationModel;

namespace PunchingBand.Models
{
    public class HistoryModel : PersistentModelBase
    {
        private ObservableCollection<HistoryInfo> records;
        private ObservableCollection<HistoryInfo> sortedFilteredRecords;

        private GameMode gameMode = GameMode.MiniGame;

        public HistoryModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                // In designer create some fake data for visualization
                records = new ObservableCollection<HistoryInfo>
                {
                    new HistoryInfo { GameMode = GameMode.MiniGame, Duration = TimeSpan.FromSeconds(30), Score = 10, CaloriesBurned = 5, Timestamp = DateTime.Now.AddDays(-1) },
                    new HistoryInfo { GameMode = GameMode.MiniGame, Duration = TimeSpan.FromSeconds(30), Score = 20, CaloriesBurned = 15, Timestamp = DateTime.Now.AddDays(-2) },
                    new HistoryInfo { GameMode = GameMode.MiniGame, Duration = TimeSpan.FromSeconds(30), Score = 30, CaloriesBurned = 95, Timestamp = DateTime.Now.AddDays(-3) },
                    new HistoryInfo { GameMode = GameMode.MiniGame, Duration = TimeSpan.FromSeconds(30), Score = 40, CaloriesBurned = 145, Timestamp = DateTime.Now.AddDays(-4) },
                    new HistoryInfo { GameMode = GameMode.MiniGame, Duration = TimeSpan.FromSeconds(30), Score = 50, CaloriesBurned = 75, Timestamp = DateTime.Now.AddDays(-5) },
                    new HistoryInfo { GameMode = GameMode.MiniGame, Duration = TimeSpan.FromSeconds(30), Score = 60, CaloriesBurned = 85, Timestamp = DateTime.Now.AddDays(-6) },
                };
                Records = new ObservableCollection<HistoryInfo>(records);
            }
            else
            {
                records = new ObservableCollection<HistoryInfo>();
                Records = new ObservableCollection<HistoryInfo>(records);

                PropertyChanged += async (s, e) => await Save();
            }
        }

        public GameMode GameMode
        {
            get { return gameMode;  }
            set
            {
                if (Set("GameMode", ref gameMode, value))
                {
                    UpdateSortAndFilter();
                }
            }
        }

        public ObservableCollection<HistoryInfo> Records
        {
            get { return records; }
            private set
            {
                if (records != null)
                {
                    records.CollectionChanged -= RecordsOnCollectionChanged;
                }

                Set("Records", ref records, value);

                UpdateSortAndFilter();

                records.CollectionChanged += RecordsOnCollectionChanged;
            }
        }

        private async void RecordsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            UpdateSortAndFilter();

            if (!DesignMode.DesignModeEnabled)
            {
                await Save();     
            }
        }

        [JsonIgnore]
        public ObservableCollection<HistoryInfo> SortedFilteredRecords
        {
            get { return sortedFilteredRecords; }
            private set { Set("SortedFilteredRecords", ref sortedFilteredRecords, value); }
        }

        private void UpdateSortAndFilter()
        {
            if (gameMode == GameMode.MiniGame)
            {
                SortedFilteredRecords = new ObservableCollection<HistoryInfo>(
                    records.Where(r => r.GameMode == gameMode).OrderByDescending(r => r.Score));
            }
            else
            {
                SortedFilteredRecords = new ObservableCollection<HistoryInfo>(
                    records.Where(r => r.GameMode == gameMode).OrderByDescending(r => r.Timestamp));
            }
        }
    }
}
