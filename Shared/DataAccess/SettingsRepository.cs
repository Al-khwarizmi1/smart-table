using System.Linq;
using Shared.Entities;

namespace Shared.DataAccess
{
    public class SettingsRepository
    {
        DataContext _context;

        public SettingsRepository()
        {
            _context = new DataContext();
        }

        public Settings LastEntrie()
        {
            var current = _context.Settings
                .OrderByDescending(x => x.Id)
                .Take(1)
                .FirstOrDefault();

            var defaultSettings = new Settings
            {
                ArduinoComPort = "COM3",
                IntervalLength = 5
            };

            return current ?? defaultSettings;
        }
    }
}
