using System;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using NLog;
using Shared.DataAccess;
using Shared.Entities;

namespace ArduinoService
{
    public partial class ArduinoService : ServiceBase
    {
        private SensorDataRepository _sensorData;
        private SensorData _lastData;
        private Logger _logger;

        public ArduinoService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _sensorData = new SensorDataRepository();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Service started");
            _lastData = _sensorData.LastEntrie();

            _logger.Info(GetPropertyValues(_lastData) ?? "No last data found");

        }

        protected override void OnStop()
        {
            _logger.Info("Service stopped");
        }


        private string GetPropertyValues(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var t = obj.GetType();

            var values = String.Join(",", t.GetProperties(BindingFlags.Public)
                .ToList()
                .Select(x => x.Name + "-" + x.GetValue(t)));

            return values;
        }

    }



}
