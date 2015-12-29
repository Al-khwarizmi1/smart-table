using System;
using System.Diagnostics;
using System.IO;
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

        public static string BaseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);


        private NotifyIcon _sysTrayIcon;
        private SensorDataService _service;

        public MainWindow()
        {
            InitializeComponent();
            _service = new SensorDataService();

            CreateIcon();
            _service.Start();
            try
            {
                EnsureAppIsInStartup();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
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

            return new MenuItem[] {
                restart,exit
            };
        }
    }
}
