using System;
using Microsoft.Win32;
using NLog;

namespace ClearModifications
{
    public class Program
    {
        private static Logger _logger;

        static void Main()
        {
            _logger = LogManager.GetCurrentClassLogger();

            RemoveFromStartUp();
        }

        private static void RemoveFromStartUp()
        {
            var registryName = "Smart Table SensorDataCollector";

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.DeleteValue(registryName);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
            }
        }
    }
}
