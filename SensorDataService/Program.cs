using System.ServiceProcess;

namespace SensorDataService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SensorDataService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
