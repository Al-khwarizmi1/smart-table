using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using NLog;

namespace Client.WPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private SensorDataAggregation _aggregation;
        private System.Timers.Timer _timer;
        private Logger _logger;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SensorDataAggregateViewModel> Today { get; private set; }
        public ObservableCollection<SensorDataAggregateViewModel> RunningWeek { get; private set; }
        public ObservableCollection<SensorDataAggregateViewModel> Last30Days { get; private set; }

        public int Balance { get; set; }
        public string BalanceString { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _logger = LogManager.GetCurrentClassLogger();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

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

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.ExceptionObject);
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

            if (last30Days.StandMinutes + last30Days.SitMinutes == 0)
            {
                BalanceString = "No captured data";
            }
            else
            {
                var total = last30Days.SitMinutes + last30Days.StandMinutes;
                BalanceString = $"From {ToTime(last30Days.SitMinutes + last30Days.StandMinutes)} in total, {ToTime(last30Days.StandMinutes)} spent standing, ratio is {Balance}%";
                if (Balance < 30)
                {
                    var required = (int)(total * 0.3 - last30Days.StandMinutes);

                    BalanceString += $", {ToTime(required)} required to reach goal";
                }
            }

            NotifyPropertyChanged(nameof(Today));
            NotifyPropertyChanged(nameof(RunningWeek));
            NotifyPropertyChanged(nameof(Last30Days));
            NotifyPropertyChanged(nameof(BalanceString));
        }

        private void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        private string ToTime(int minutes)
        {
            var span = TimeSpan.FromMinutes(minutes);
            return $"{span.Hours:00}h {span.Minutes:00}m";
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
