using System;
using System.IO.Ports;
using System.ServiceProcess;
using NLog;
using Shared;
using Shared.DataAccess;
using Shared.Entities;

namespace ArduinoService
{
    public partial class ArduinoService : ServiceBase
    {
        private SensorDataRepository _sensorData;
        private SensorData _lastData;
        private Logger _logger;
        private System.Timers.Timer _timer;
        const string GetHeight = "1";

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

            _logger.Info(Helpers.GetPropertyValues(_lastData) ?? "No last data found");

            serialPort1.PortName = "COM3";
            serialPort1.BaudRate = 9600;
            serialPort1.DataReceived += _serialPort_DataReceived;

            _timer = new System.Timers.Timer();
            _timer.Interval = 5000;
            _timer.Elapsed += _timer_Elapsed;

            _timer.Start();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MonitorHeight();
        }

        private void MonitorHeight()
        {
            try
            {
                if (serialPort1.IsOpen == false)
                {
                    serialPort1.Open();
                }

                serialPort1.Write(GetHeight);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = new SensorData { DateTime = DateTime.UtcNow, Height = Int32.Parse(serialPort1.ReadLine()) };
            if (_lastData == null
                || Math.Abs(_lastData.Height - data.Height) > 5
                || (data.DateTime - _lastData.DateTime) > TimeSpan.FromMinutes(5))
            {
                _sensorData.Add(data);
                _lastData = data;
            }
        }

        protected override void OnStop()
        {
            _timer.Stop();
            if (_lastData.Id == 0)
            {
                try
                {
                    _sensorData.Add(_lastData);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "OnStop");
                }
            }
            _logger.Info("Service stopped");
        }

    }

}
