using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using NLog;
using Shared;
using Shared.DataAccess;
using Shared.Entities;

namespace SensorDataService
{
    public partial class SensorDataService : ServiceBase
    {
        private const string GetHeight = "1";

        private SensorDataRepository _sensorDataRepository;
        private SensorData _lastData;
        private Logger _logger;
        private System.Timers.Timer _timer;

        private DateTime _currentPeriodVal;
        private DateTime _currentPeriod
        {
            get { return _currentPeriodVal; }
            set
            {
                _currentPeriodVal = value;
                _currentPeriodEnd = _currentPeriod.Add(_periodLength);
            }
        }

        private DateTime _currentPeriodEnd;
        private TimeSpan _periodLength;
        private bool _periodHasInputData;

        private IList<int> _heights;

        public SensorDataService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _sensorDataRepository = new SensorDataRepository();
            _logger = LogManager.GetCurrentClassLogger();
            _heights = new List<int>();

            _periodLength = TimeSpan.FromMinutes(5);

            _logger.Info("Service started");
            _lastData = _sensorDataRepository.LastEntrie();

            _logger.Info(Helpers.GetPropertyValues(_lastData) ?? "No last data found");

            serialPort1.PortName = "COM3";
            serialPort1.BaudRate = 9600;
            serialPort1.DataReceived += serialPort1_DataReceived;

            _timer = new System.Timers.Timer();
            _timer.Interval = 5000;
            _timer.Elapsed += _timer_Elapsed;


            //In case database has newer period as handled, to prevent having same period twice
            if (_lastData != null && _lastData.DateTime.Add(_periodLength) > DateTime.UtcNow)
            {
                _currentPeriod = _lastData.DateTime;
                _periodHasInputData = true;
                _logger.Info("Current period loaded:" + _currentPeriod.ShortDateTime());
            }
            else
            {
                _currentPeriod = GetCurrentPeriod();
                _periodHasInputData = false;
                _logger.Info("Current period set:" + _currentPeriod.ShortDateTime());
            }

            _timer.Start();
        }

        private DateTime GetCurrentPeriod()
        {
            var date = DateTime.UtcNow;
            var period = (int)_periodLength.TotalMinutes;

            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute - (date.Minute % period), 0);
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_currentPeriodEnd > DateTime.UtcNow)
            {
                if (_periodHasInputData)
                {
                    _sensorDataRepository.Add(new SensorData { DateTime = _currentPeriod, Height = (int)_heights.Average() });
                }

                _logger.Info($"Old period:{_currentPeriod}, total  heights:{_heights.Count}, has input data:{_periodHasInputData}");

                //Reset for new period
                _heights.Clear();
                _periodHasInputData = false;
                _currentPeriod = GetCurrentPeriod();
            }

            MonitorLastInput();

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

        private void MonitorLastInput()
        {
            if (_periodHasInputData == false)
            {
                var lastInput = GetLastInputDate();
                if (_currentPeriod < lastInput && lastInput < _currentPeriodEnd)
                {
                    _periodHasInputData = true;
                    _logger.Info($"Last input: {lastInput.ShortDateTime()}");
                }
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var height = Int32.Parse(serialPort1.ReadLine());

            if (height < 40 || 200 < height)
            {
                _logger.Info($"Invalid table height: {height}");
                return;
            }

            _heights.Add(height);
            _logger.Info($"Recieved height {height}, total list length:{_heights.Count}");
        }

        private DateTime GetLastInputDate()
        {
            var lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            GetLastInputInfo(ref lastInputInfo);

            var lastInput = DateTime.Now.AddMilliseconds(-(Environment.TickCount - lastInputInfo.dwTime));

            return lastInput.ToUniversalTime();
        }

        protected override void OnStop()
        {
            _timer.Stop();
            serialPort1.Close();
            _logger.Info("Service stopped");
        }

        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

    }

}
