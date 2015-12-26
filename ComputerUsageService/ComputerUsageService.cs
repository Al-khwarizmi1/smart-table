using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using NLog;
using Shared;
using Shared.DataAccess;
using Shared.Entities;

namespace ComputerUsageService
{
    public partial class ComputerUsageService : ServiceBase
    {
        Logger _logger;
        ComputerUsageRepository _repository;

        DateTime _currentPeriod;
        ComputerUsageData _lastData;
        TimeSpan _periodLengt;
        bool _periodHandled;

        private System.Timers.Timer _timer;

        public ComputerUsageService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _repository = new ComputerUsageRepository();


            _periodLengt = TimeSpan.FromMinutes(5);
            _lastData = _repository.LastEntrie();

            _timer = new System.Timers.Timer();
            _timer.Interval = 5000;
            _timer.Elapsed += _timer_Elapsed;


            //In case database has newer period as handled, to prevent having same period twice
            if (_lastData != null && _lastData.DateTime.Add(_periodLengt) > DateTime.UtcNow)
            {
                _currentPeriod = _lastData.DateTime.Add(_periodLengt);
                _periodHandled = true;
            }
            else
            {
                _currentPeriod = GetCurrentPeriod();
                _periodHandled = false;
            }

            _logger.Info("Service started");
            _logger.Info(Helpers.GetPropertyValues(_currentPeriod) ?? "No last data found");
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_currentPeriod.Add(_periodLengt) < DateTime.UtcNow)
            {
                var oldPeriod = _currentPeriod;
                _periodHandled = false;
                _currentPeriod = GetCurrentPeriod();
                _logger.Info($"New period from: {oldPeriod}, to: {_currentPeriod}");
            }

            if (_periodHandled == false)
            {
                var lastInputInfo = new LASTINPUTINFO();
                lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

                GetLastInputInfo(ref lastInputInfo);

                var lastInput = DateTime.Now.AddMilliseconds(-(Environment.TickCount - lastInputInfo.dwTime));

                if (_currentPeriod > lastInput.ToUniversalTime() && lastInput.ToUniversalTime() < _currentPeriod.Add(_periodLengt))
                {
                    HandlePeriod();
                }
            }

        }

        private DateTime GetCurrentPeriod()
        {
            var date = DateTime.UtcNow;
            var period = (int)_periodLengt.TotalMinutes;

            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute - (date.Minute % period), 0);
        }

        private void HandlePeriod()
        {
            if (_periodHandled) { return; }
            _periodHandled = true;
            _repository.Add(new ComputerUsageData { DateTime = _currentPeriod });
        }

        protected override void OnStop()
        {
            _timer.Stop();
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
