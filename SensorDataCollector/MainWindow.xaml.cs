using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using Shared;

namespace SensorDataCollector
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal extern static bool DestroyIcon(IntPtr handle);

        private NotifyIcon _sysTrayIcon;
        private SensorDataService _service;

        public MainWindow()
        {
            InitializeComponent();
            _service = new SensorDataService();

            CreateIcon();
            _service.Start();
            EnsureAppIsInStartup();
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
            _sysTrayIcon.Icon = new System.Drawing.Icon(@"Resources\table.ico", 40, 40);
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

            return new MenuItem[] {
                exit
            };
        }
    }
}
