using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        public ObservableCollection<SensorDataAggregateViewModel> TodayPerc { get; private set; }
        public ObservableCollection<SensorDataAggregateViewModel> Last7DaysPerc { get; private set; }
        public ObservableCollection<SensorDataAggregateViewModel> Last30DaysPerc { get; private set; }

        public string TodayTitle { get; set; }
        public string Last7DaysTitle { get; set; }
        public string Last30DaysTitle { get; set; }

        public int Balance { get; set; }
        public string BalanceString { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _logger = LogManager.GetCurrentClassLogger();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            TodayPerc = new ObservableCollection<SensorDataAggregateViewModel>();
            Last7DaysPerc = new ObservableCollection<SensorDataAggregateViewModel>();
            Last30DaysPerc = new ObservableCollection<SensorDataAggregateViewModel>();

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
            var last7Days = _aggregation.RunningWeek();
            var last30Days = _aggregation.Last30Days();
            var last30DaysGrouped = _aggregation.GroupedByDays();
            var last14DaysGrouped = last30DaysGrouped.Take(14);


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
                BalanceString = $"From {ToTime(total)} in total, {ToTime(last30Days.StandMinutes)} spent standing, ratio is {Balance}%";
                if (Balance < 30)
                {
                    var required = (int)(total * 0.3 - last30Days.StandMinutes);

                    BalanceString += $", {ToTime(required)} required to reach goal";
                }
            }

            TodayPerc = ToViewModelPercent(today);
            Last7DaysPerc = ToViewModelPercent(last7Days);
            Last30DaysPerc = ToViewModelPercent(last30Days);

            TodayTitle = $"Today [{ToTime(today.SitMinutes)}/{ToTime(today.StandMinutes)}]";
            Last7DaysTitle = $"Last 7 days [{ToTime(last7Days.SitMinutes)}/{ToTime(last7Days.StandMinutes)}]";
            Last30DaysTitle = $"Last 30 days [{ToTime(last30Days.SitMinutes)}/{ToTime(last30Days.StandMinutes)}]";

            NotifyPropertyChanged(nameof(TodayTitle));
            NotifyPropertyChanged(nameof(Last7DaysTitle));
            NotifyPropertyChanged(nameof(Last30DaysTitle));

            NotifyPropertyChanged(nameof(BalanceString));
            NotifyPropertyChanged(nameof(TodayPerc));
            NotifyPropertyChanged(nameof(Last7DaysPerc));
            NotifyPropertyChanged(nameof(Last30DaysPerc));


            Application.Current.Dispatcher.Invoke(() =>
            {
                var series = new ObservableCollection<De.TorstenMandelkow.MetroChart.ChartSeries>();
                var dayFiller = new ObservableCollection<De.TorstenMandelkow.MetroChart.ChartSeries>(
                    Enumerable
                    .Range(0, 14)
                    .Select(x => DateTime.Now.Date.AddDays(x * -1))
                    .OrderBy(x => x)
                    .Select(x => new De.TorstenMandelkow.MetroChart.ChartSeries
                    {
                        SeriesTitle = ToDate(x),
                        DisplayMember = "Category",
                        ValueMember = "Value",
                        ItemsSource = ToViewModel(new SensorDataForDay { Date = x, SitMinutes = 0, StandMinutes = 0 })
                    }));

                foreach (var day in last14DaysGrouped)
                {
                    series.Add(new De.TorstenMandelkow.MetroChart.ChartSeries
                    {
                        SeriesTitle = ToDate(day.Date),
                        DisplayMember = "Category",
                        ValueMember = "Value",
                        ItemsSource = ToViewModel(day)
                    });
                }

                var filledEmptyDays = new ObservableCollection<De.TorstenMandelkow.MetroChart.ChartSeries>(
                    dayFiller
                    .Select(x => series.FirstOrDefault(a => a.SeriesTitle == x.SeriesTitle) ?? x));

                StackedColumns.Series = filledEmptyDays;
            });
        }

        private void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private string ToTime(int minutes)
        {
            var span = TimeSpan.FromMinutes(minutes);
            if ((int)span.TotalDays > 0)
            {
                return $"{(int)span.TotalDays}d {span.Hours:00}h {span.Minutes:00}m";
            }
            else
            {
                return $"{span.TotalHours:00}h {span.Minutes:00}m";
            }
        }

        private string ToDate(DateTime date)
        {
            return $"{date:MM-dd}";
        }

        private ObservableCollection<SensorDataAggregateViewModel> ToViewModel(SensorDataForDay data)
        {
            var result = new ObservableCollection<SensorDataAggregateViewModel>();

            result.Add(new SensorDataAggregateViewModel { Category = "Sitting", Value = data.SitMinutes });
            result.Add(new SensorDataAggregateViewModel { Category = "Standing", Value = data.StandMinutes });

            return result;
        }

        private ObservableCollection<SensorDataAggregateViewModel> ToViewModelPercent(SensorDataForDay data)
        {
            var result = new ObservableCollection<SensorDataAggregateViewModel>();

            result.Add(new SensorDataAggregateViewModel { Category = BalanceString, Value = (int)((data.SitMinutes / (double)(data.SitMinutes + data.StandMinutes)) * 100) });

            return result;
        }

    }

    public class SensorDataAggregateViewModel
    {
        public string Category { get; set; }
        public int Value { get; set; }
    }

}
