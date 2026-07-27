using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Localization;
using ExShrinkSidebar.Script.Utils;
using ExShrinkSidebar.UI.Views.ContextMenu;
using ExShrinkSidebar.UI.Views.MainWindow;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using Application = System.Windows.Application;
using Forms = System.Windows.Forms;

namespace ExShrinkSidebar
{
    public partial class App : Application
    {
        private Forms.NotifyIcon _notifyIcon;
        private ContextMenuManager _trayMenuManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;

            LocalizationHelper.ApplyCulture(CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? "zh-CN"
                : "en-US");

            DockState.SetEdge(DockEdge.Top);
            var mainWindow = CreateMainWindow();

            SetupNotifyIcon();

            mainWindow.Show();
        }

        public void RecreateMainWindow()
        {
            MainWindow?.Close();
            CreateMainWindow().Show();
        }

        private MainWindow CreateMainWindow()
        {
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            return mainWindow;
        }
        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            base.OnExit(e);
        }

        //≥ı ºªØÕ–≈ÃÕº±Í
        private void SetupNotifyIcon()
        {
            _notifyIcon = new Forms.NotifyIcon
            {
                Visible = true,
                Text = "ExShrinkSidebar",
                Icon = new Icon(ResHelper.GetPath("exicon256.ico"))
            };

            _trayMenuManager = new ContextMenuManager(_notifyIcon);
        }
    }
}
