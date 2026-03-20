using ExShrinkSidebar.Script.Core.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace ExShrinkSidebar.UI.ViewModels.Common
{
    /// <summary>
    /// 简化的 Window 基类
    /// </summary>
    public abstract class WindowBase : Window
    {
        private bool _isCleanedUp = false;

        protected WindowBase()
        {
            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;
        }

        /// <summary>
        /// 窗口加载完成
        /// </summary>
        protected virtual void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
        }

        /// <summary>
        /// 注册事件监听 - 子类重写
        /// </summary>
        protected virtual void RegisterEvents()
        {
        }

        /// <summary>
        /// 窗口关闭事件处理器
        /// </summary>
        protected virtual void OnWindowClosed(object sender, EventArgs e)
        {
            Cleanup();
        }

        /// <summary>
        /// 重写 OnClosed 方法
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            Cleanup();
            base.OnClosed(e);
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected virtual void Cleanup()
        {
            if (_isCleanedUp) return;
            _isCleanedUp = true;

            // 移除所有事件监听
            EVENT.targetOff(this);

            // 移除事件 - 使用新的事件处理器名称
            Loaded -= OnWindowLoaded;
            Closed -= OnWindowClosed;
        }

        #region 辅助方法

        /// <summary>
        /// 在 UI 线程上执行操作
        /// </summary>
        protected void RunOnUIThread(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.Invoke(action);
            }
        }

        /// <summary>
        /// 异步在 UI 线程上执行操作
        /// </summary>
        protected void BeginInvokeOnUIThread(Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.BeginInvoke(action);
            }
        }

        #endregion
    }
}
