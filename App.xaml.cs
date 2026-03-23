using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Localization;
using ExShrinkSidebar.Script.Utils;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;

            LocalizationHelper.ApplyCulture(CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? "zh-CN"
                : "en-US");

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            SetupNotifyIcon();
            mainWindow.Show();
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

        private void SetupNotifyIcon()
        {
            _notifyIcon = new Forms.NotifyIcon
            {
                Visible = true,
                Text = "ExShrinkSidebar",
                Icon = new Icon(ResHelper.GetPath("terminal.ico", "Asset/Resource/Icons"))
            };
            RebuildTrayMenu();
        }

        private void RebuildTrayMenu()
        {
            var menu = new Forms.ContextMenuStrip();
            var languageMenu = new Forms.ToolStripMenuItem(UiTextCatalog.Get(UiTextCatalog.TrayLanguageMenu));

            var zhItem = CreateLanguageMenuItem("zh-CN", UiTextCatalog.Get(UiTextCatalog.LanguageZhCn));
            var enItem = CreateLanguageMenuItem("en-US", UiTextCatalog.Get(UiTextCatalog.LanguageEnUs));

            languageMenu.DropDownItems.Add(zhItem);
            languageMenu.DropDownItems.Add(enItem);
            menu.Items.Add(languageMenu);
            menu.Items.Add(new Forms.ToolStripSeparator());
            var exitItem = new Forms.ToolStripMenuItem(UiTextCatalog.Get(UiTextCatalog.TrayExit));
            exitItem.Click += (_, __) => Shutdown();
            menu.Items.Add(exitItem);
            _notifyIcon.ContextMenuStrip = menu;
        }

        private Forms.ToolStripMenuItem CreateLanguageMenuItem(string cultureName, string text)
        {
            var menuItem = new Forms.ToolStripMenuItem(text)
            {
                Checked = string.Equals(Asset.Properties.StringResources.Culture?.Name ?? CultureInfo.CurrentUICulture.Name, cultureName, StringComparison.OrdinalIgnoreCase)
            };
            menuItem.Click += (_, __) =>
            {
                LocalizationHelper.ApplyCulture(cultureName);
                RebuildTrayMenu();
                EVENT.emit(EventIds.ON_LANGUAGE_CHANGED, this, new LanguageChangedEventArg { CultureName = cultureName });
            };
            return menuItem;
        }
    }
}
