using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PunchingBand.Models
{
    public class HistoryModel : ModelBase
    {
        private ObservableCollection<HistoryInfo> records;
        private ObservableCollection<HistoryInfo> sortedRecords;

        public HistoryModel()
        {
            records = new ObservableCollection<HistoryInfo>
            {
                new HistoryInfo { Duration = TimeSpan.FromSeconds(15), Score = 10, Timestamp = DateTime.Now.AddDays(-1) },
                new HistoryInfo { Duration = TimeSpan.FromSeconds(15), Score = 20, Timestamp = DateTime.Now.AddDays(-2) },
                new HistoryInfo { Duration = TimeSpan.FromSeconds(15), Score = 30, Timestamp = DateTime.Now.AddDays(-3) },
                new HistoryInfo { Duration = TimeSpan.FromSeconds(15), Score = 40, Timestamp = DateTime.Now.AddDays(-4) },
                new HistoryInfo { Duration = TimeSpan.FromSeconds(15), Score = 50, Timestamp = DateTime.Now.AddDays(-5) },
                new HistoryInfo { Duration = TimeSpan.FromSeconds(15), Score = 60, Timestamp = DateTime.Now.AddDays(-6) },
            };

            Records = new ObservableCollection<HistoryInfo>(records);
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

                SortedRecords = new ObservableCollection<HistoryInfo>(records.OrderBy(r => r.Score));
                records.CollectionChanged += RecordsOnCollectionChanged;
            }
        }

        private void RecordsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            SortedRecords = new ObservableCollection<HistoryInfo>(records.OrderBy(r => r.Score));
        }

        public ObservableCollection<HistoryInfo> SortedRecords
        {
            get { return sortedRecords; }
            private set { Set("SortedRecords", ref sortedRecords, value); }
        }
    }
}
