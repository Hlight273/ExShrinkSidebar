using ExShrinkSidebar.Script.Core.Event;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace ExShrinkSidebar.UI.ViewModels.Common
{
    /// <summary>
    /// Window 基类 - 支持双向绑定和事件管理
    /// </summary>
    /// <typeparam name="TViewModel">ViewModel 类型，必须继承自 ViewModelBase</typeparam>
    public abstract class WindowBase<TViewModel> : Window where TViewModel : ViewModelBase, new()
    {
        private TViewModel _viewModel;
        private bool _isCleanedUp = false;

        /// <summary>
        /// 获取或设置 ViewModel
        /// </summary>
        public TViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel != value)
                {
                    // 清理旧的 ViewModel
                    if (_viewModel != null)
                    {
                        _viewModel.Cleanup();
                        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                    }

                    _viewModel = value;
                    DataContext = _viewModel;

                    if (_viewModel != null)
                    {
                        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
                    }

                    OnViewModelChanged();
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        protected WindowBase()
        {
            // 创建 ViewModel 实例
            ViewModel = new TViewModel();

            // 注册窗口事件
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Closed += OnClosed;
        }

        #region 生命周期方法

        /// <summary>
        /// 窗口加载完成
        /// </summary>
        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 注册事件监听
            RegisterEvents();
        }

        /// <summary>
        /// 窗口卸载
        /// </summary>
        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // 不需要在这里清理，Closed 事件会处理
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        protected virtual void OnClosed(object sender, EventArgs e)
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

        #endregion

        #region 事件管理

        /// <summary>
        /// 注册事件监听 - 子类重写此方法注册事件
        /// </summary>
        protected virtual void RegisterEvents()
        {
            // 子类重写此方法注册事件
        }

        /// <summary>
        /// 清理所有资源
        /// </summary>
        protected virtual void Cleanup()
        {
            if (_isCleanedUp) return;
            _isCleanedUp = true;

            // 移除所有事件监听
            EVENT.targetOff(this);

            // 清理 ViewModel
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.Cleanup();
            }

            // 移除窗口事件
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
            Closed -= OnClosed;
        }

        #endregion

        #region ViewModel 相关

        /// <summary>
        /// ViewModel 属性变化时触发
        /// </summary>
        protected virtual void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 子类可以重写此方法响应 ViewModel 属性变化
        }

        /// <summary>
        /// ViewModel 实例变化时触发
        /// </summary>
        protected virtual void OnViewModelChanged()
        {
            // 子类可以重写此方法响应 ViewModel 变化
        }

        #endregion

        #region 便捷方法

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
