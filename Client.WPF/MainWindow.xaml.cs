using System.Collections.ObjectModel;
using System.Windows;

namespace Client.WPF
{
    public partial class MainWindow : Window
    {
        private SensorDataAggregation _aggregation;

        public ObservableCollection<SensorDataAggregateViewModel> Today { get; private set; }
        public ObservableCollection<SensorDataAggregateViewModel> RunningWeek { get; private set; }
        public ObservableCollection<SensorDataAggregateViewModel> Last30Days { get; private set; }

        public int Balance { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _aggregation = new SensorDataAggregation();
            Today = new ObservableCollection<SensorDataAggregateViewModel>();
            RunningWeek = new ObservableCollection<SensorDataAggregateViewModel>();
            Last30Days = new ObservableCollection<SensorDataAggregateViewModel>();

            var today = _aggregation.Today();
            var runningWeek = _aggregation.RunningWeek();
            var last30Days = _aggregation.Last30Days();

            Today = ToViewModel(today);
            RunningWeek = ToViewModel(runningWeek);
            Last30Days = ToViewModel(last30Days);

            Balance = last30Days.StandMinutes == 0
                ? 0
                : (last30Days.SitMinutes + last30Days.StandMinutes) / last30Days.StandMinutes * 100;

            DataContext = this;
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
