using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PunchingBand.Models
{
    public class HistoryModel : ModelBase
    {
        private ObservableCollection<HistoryInfo> records;

        public HistoryModel()
        {
            records = new ObservableCollection<HistoryInfo>
            {
                new HistoryInfo { Duration = TimeSpan.FromSeconds(15), Score = 10 },
                new HistoryInfo { Duration = TimeSpan.FromSeconds(15), Score = 20 },
                new HistoryInfo { Duration = TimeSpan.FromSeconds(15), Score = 30 },
            };

            records = new ObservableCollection<HistoryInfo>(records.OrderByDescending(h => h.Score));
            records.CollectionChanged += RecordsOnCollectionChanged;
        }

        private void RecordsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            Records = new ObservableCollection<HistoryInfo>(records.OrderByDescending(h => h.Score));
        }

        public ObservableCollection<HistoryInfo> Records
        {
            get { return records; }
            set { Set("Records", ref records, value); }
        }
    }
}
