using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Client.WPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private SensorDataAggregation _aggregation;
        private System.Timers.Timer _timer;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SensorDataAggregateViewModel> Today { get; private set; }
        public ObservableCollection<SensorDataAggregateViewModel> RunningWeek { get; private set; }
        public ObservableCollection<SensorDataAggregateViewModel> Last30Days { get; private set; }

        public int Balance { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Today = new ObservableCollection<SensorDataAggregateViewModel>();
            RunningWeek = new ObservableCollection<SensorDataAggregateViewModel>();
            Last30Days = new ObservableCollection<SensorDataAggregateViewModel>();
            _timer = new System.Timers.Timer();
            _timer.Interval = TimeSpan.FromMinutes(2).TotalMilliseconds;
            _timer.Elapsed += _timer_Elapsed;

            Refresh();

            _timer.Start();

            DataContext = this;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            _aggregation = new SensorDataAggregation();

            _aggregation.Reload();

            var today = _aggregation.Today();
            var runningWeek = _aggregation.RunningWeek();
            var last30Days = _aggregation.Last30Days();

            Today = ToViewModel(today);
            RunningWeek = ToViewModel(runningWeek);
            Last30Days = ToViewModel(last30Days);

            Balance = last30Days.StandMinutes == 0
                ? 0
                : (int)((double)last30Days.StandMinutes / (double)(last30Days.SitMinutes + last30Days.StandMinutes) * 100.0);

            NotifyPropertyChanged(nameof(Today));
            NotifyPropertyChanged(nameof(RunningWeek));
            NotifyPropertyChanged(nameof(Last30Days));
            NotifyPropertyChanged(nameof(Balance));
        }

        private void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        private ObservableCollection<SensorDataAggregateViewModel> ToViewModel(SensorDataForDay data)
        {
            var result = new ObservableCollection<SensorDataAggregateViewModel>();

            result.Add(new SensorDataAggregateViewModel { Category = "Sit", Value = data.SitMinutes });
            result.Add(new SensorDataAggregateViewModel { Category = "Stand", Value = data.StandMinutes });

            return result;
        }

    }

    public class SensorDataAggregateViewModel
    {
        public string Category { get; set; }
        public int Value { get; set; }
    }

}
