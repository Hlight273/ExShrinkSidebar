using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExShrinkSidebar.UI.Views.ContextMenu
{
    public class ContextMenuManager
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _menu;

        public ContextMenuManager(NotifyIcon notifyIcon)
        {
            _notifyIcon = notifyIcon;
            _menu = new ContextMenuStrip();
            BuildMenu();
        }

        private void BuildMenu()
        {
            //清空旧菜单
            _menu.Items.Clear();

            //切换语言功能
            var languageMenu = new ToolStripMenuItem(UiTextCatalog.Get(UiTextCatalog.TrayLanguageMenu));
            var zhItem = CreateLanguageMenuItem("zh-CN", UiTextCatalog.Get(UiTextCatalog.LanguageZhCn));
            var enItem = CreateLanguageMenuItem("en-US", UiTextCatalog.Get(UiTextCatalog.LanguageEnUs));
            languageMenu.DropDownItems.Add(zhItem);
            languageMenu.DropDownItems.Add(enItem);
            _menu.Items.Add(languageMenu);

            //line
            _menu.Items.Add(new ToolStripSeparator());

            //切换显示屏功能
            var screenMenu = new ToolStripMenuItem("更新所处显示屏");
            screenMenu.DropDownOpening += OnScreenMenuOpening; //在鼠标移入时动态生成列表
            _menu.Items.Add(screenMenu);

            //line
            _menu.Items.Add(new ToolStripSeparator());

            //退出功能
            var exitItem = new ToolStripMenuItem(UiTextCatalog.Get(UiTextCatalog.TrayExit));
            exitItem.Click += (s, e) => System.Windows.Application.Current.Shutdown();
            _menu.Items.Add(exitItem);

            //设置新菜单
            _notifyIcon.ContextMenuStrip = _menu;
        }

        //动态生成显示器列表
        private void OnScreenMenuOpening(object sender, EventArgs e)
        {
            var parentItem = sender as ToolStripMenuItem;
            if (parentItem == null) return;

            parentItem.DropDownItems.Clear();

            var screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                var item = new ToolStripMenuItem($"显示器 {i}");
                int index = i;
                item.Click += (s, args) => OnChangeScreenClicked(index);//闭包index
                parentItem.DropDownItems.Add(item);
            }

            //兜底
            if (screens.Length == 0)
            {
                var emptyItem = new ToolStripMenuItem("无可用显示器");
                emptyItem.Enabled = false;
                parentItem.DropDownItems.Add(emptyItem);
            }
        }

        //点击显示器菜单项
        private void OnChangeScreenClicked(int screenIndex)
        {
            DockState.SetScreenIndex(screenIndex);
        }

        private ToolStripMenuItem CreateLanguageMenuItem(string cultureName, string text)
        {
            var currentCulture = Asset.Properties.StringResources.Culture?.Name
                                 ?? System.Globalization.CultureInfo.CurrentUICulture.Name;

            var menuItem = new ToolStripMenuItem(text)
            {
                Checked = string.Equals(currentCulture, cultureName, StringComparison.OrdinalIgnoreCase)
            };
            menuItem.Click += (_, __) =>
            {
                LocalizationHelper.ApplyCulture(cultureName);
                RebuildMenu(); //语言变更后重建菜单以刷新文本
                EVENT.emit(EventIds.ON_LANGUAGE_CHANGED, this, new LanguageChangedEventArg { CultureName = cultureName });
            };
            return menuItem;
        }

        //供外部调用
        public void RebuildMenu()
        {
            BuildMenu();
        }
    }
}
