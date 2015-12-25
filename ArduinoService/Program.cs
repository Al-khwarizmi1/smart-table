using System.ServiceProcess;

namespace ArduinoService
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
                new ArduinoService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
