using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DataAccess;
using Shared.Entities;

namespace Client.WPF
{
    public class SensorDataAggregation
    {
        SensorDataRepository _repository;
        SettingsRepository _settings;
        Settings _currentSettings;

        IEnumerable<SensorData> _sensorData;

        public SensorDataAggregation()
        {
            _settings = new SettingsRepository();
            _repository = new SensorDataRepository();
            Reload();
        }

        public void Reload()
        {
            _currentSettings = _settings.LastEntrie();

            _sensorData = _repository.GetNewerThen(DateTime.UtcNow.Date.AddDays(-30)).ToList();

            foreach (var item in _sensorData)
            {
                item.DateTime = item.DateTime.ToLocalTime();
            }
        }

        public SensorDataForDay Today()
        {
            var today = DateTime.Now.Date;
            var todayData = _sensorData.Where(x => x.DateTime.Date == today);

            var result = AggregateList(todayData);
            result.Date = today;

            return result;
        }

        public SensorDataForDay RunningWeek()
        {
            var date = DateTime.Now.Date.AddDays(-7);
            var data = _sensorData.Where(x => x.DateTime.Date >= date);

            var result = AggregateList(data);
            result.Date = date;

            return result;
        }

        public SensorDataForDay Last30Days()
        {
            var date = DateTime.Now.Date.AddDays(-30);

            var data = _sensorData.Where(x => x.DateTime.Date >= date);

            var result = AggregateList(data);
            result.Date = date;

            return result;
        }

        private SensorDataForDay AggregateList(IEnumerable<SensorData> data)
        {
            var result = new SensorDataForDay
            {
                SitMinutes = data.Where(x => x.Height <= _currentSettings.StandSitSeparation).Sum(a => a.IntervalLength),
                StandMinutes = data.Where(x => x.Height > _currentSettings.StandSitSeparation).Sum(a => a.IntervalLength)
            };

            return result;
        }

    }
}
