using System.ServiceProcess;

namespace SensorDataService
{
    public partial class SensorDataService : ServiceBase
    {
        Shared.SensorDataService _sensorDataService;
        public SensorDataService()
        {
            InitializeComponent();
            _sensorDataService = new Shared.SensorDataService();
        }

        protected override void OnStart(string[] args)
        {
            _sensorDataService.Start();
        }

        protected override void OnStop()
        {
            _sensorDataService.Stop();
        }

    }
}
