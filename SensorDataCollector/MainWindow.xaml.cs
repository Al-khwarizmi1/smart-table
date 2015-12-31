using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using NLog;
using Shared;

namespace SensorDataCollector
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal extern static bool DestroyIcon(IntPtr handle);

        private static string BaseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private Logger _logger;

        private NotifyIcon _sysTrayIcon;
        private SensorDataService _service;

        public MainWindow()
        {
            InitializeComponent();
            _service = new SensorDataService();
            _logger = LogManager.GetCurrentClassLogger();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            CreateIcon();
            _service.Start();
            try
            {
                EnsureAppIsInStartup();
            }
            catch (Exception e)
            {
                _logger.Fatal(e);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal(e.ExceptionObject);
        }

        public void EnsureAppIsInStartup()
        {
            var registryName = "Smart Table SensorDataCollector";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                object existingKey = key.GetValue(registryName, null);

                if (existingKey == null)
                {
                    key.SetValue(registryName, "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
                }
            }
        }

        public void CreateIcon()
        {
            _sysTrayIcon = new NotifyIcon();
            _sysTrayIcon.Text = "SmartTable";
            _sysTrayIcon.Icon = new System.Drawing.Icon(Path.Combine(BaseDir, @"Resources\table.ico"), 40, 40);
            _sysTrayIcon.Visible = true;

            _sysTrayIcon.ContextMenu = new ContextMenu(CreateNotifyIconContextMenu());
        }

        private MenuItem[] CreateNotifyIconContextMenu()
        {
            var exit = new MenuItem { Text = "Exit" };
            exit.Click += (o, s) =>
            {
                _service.Stop();
                Close();
            };

            var restart = new MenuItem { Text = "Restart" };
            restart.Click += (o, s) =>
            {
                _service.Stop();
                _service.Start();
            };

            var dashboard = new MenuItem { Text = "Dashboard" };
            dashboard.Click += (o, s) =>
            {
                var dashboardPath = Path.Combine(BaseDir + "\\Client.WPF.exe");

                if (File.Exists(dashboardPath))
                {
                    var startInfo = new ProcessStartInfo();
                    startInfo.FileName = dashboardPath;
                    Process.Start(startInfo);
                }
                else
                {
                    _logger.Fatal(new FileNotFoundException(dashboardPath));
                }
            };

            return new MenuItem[] {
               dashboard, restart, exit
            };
        }
    }
}
