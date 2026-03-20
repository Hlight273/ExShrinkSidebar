using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;

namespace ExShrinkSidebar.UI.Views.MainWindow
{
    /// <summary>
    /// 高性能窗口动画管理类
    /// </summary>
    public class WindowAnimator
    {
        private readonly Window _window;
        private bool _isAnimating = false;
        private Action _pendingAction;

        // 重用动画对象，避免频繁创建
        private DoubleAnimation _leftAnimation;
        private DoubleAnimation _topAnimation;
        private Storyboard _storyboard;

        // 动画参数缓存
        private double _currentTargetLeft;
        private double _currentTargetTop;
        private long _lastAnimationTime;

        // 使用 Stopwatch 替代 DateTime，提高性能
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public WindowAnimator(Window window)
        {
            _window = window;
            InitializeAnimations();
        }

        /// <summary>
        /// 是否正在播放动画
        /// </summary>
        public bool IsAnimating => _isAnimating;

        /// <summary>
        /// 初始化动画对象（只创建一次）
        /// </summary>
        private void InitializeAnimations()
        {
            // 创建可重用的动画
            _leftAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(150),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            _topAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(150),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // 使用 Storyboard 管理动画
            _storyboard = new Storyboard();
            _storyboard.Children.Add(_leftAnimation);
            _storyboard.Children.Add(_topAnimation);

            // 设置目标属性
            Storyboard.SetTarget(_leftAnimation, _window);
            Storyboard.SetTargetProperty(_leftAnimation, new PropertyPath(Window.LeftProperty));

            Storyboard.SetTarget(_topAnimation, _window);
            Storyboard.SetTargetProperty(_topAnimation, new PropertyPath(Window.TopProperty));

            // 动画完成事件
            _storyboard.Completed += (s, e) =>
            {
                _isAnimating = false;

                // 确保最终位置精确
                _window.Left = _currentTargetLeft;
                _window.Top = _currentTargetTop;

                // 执行待处理动画
                if (_pendingAction != null)
                {
                    var action = _pendingAction;
                    _pendingAction = null;
                    action();
                }
            };
        }

        /// <summary>
        /// 执行动画（带防抖）
        /// </summary>
        private void ExecuteAnimation(double targetLeft, double targetTop, double durationMs, bool force = false)
        {
            // 如果目标位置相同且不是强制动画，跳过
            if (!force &&
                Math.Abs(targetLeft - _currentTargetLeft) < 0.5 &&
                Math.Abs(targetTop - _currentTargetTop) < 0.5)
            {
                return;
            }

            // 使用 Stopwatch 获取当前时间戳（毫秒）
            var currentTime = _stopwatch.ElapsedMilliseconds;

            // 防抖：100ms 内的重复动画只执行最后一次
            if (!force && (currentTime - _lastAnimationTime) < 100)
            {
                _pendingAction = () => ExecuteAnimation(targetLeft, targetTop, durationMs, true);
                return;
            }

            // 如果正在播放动画，记录待执行
            if (_isAnimating)
            {
                _pendingAction = () => ExecuteAnimation(targetLeft, targetTop, durationMs, true);
                return;
            }

            _lastAnimationTime = currentTime;
            _currentTargetLeft = targetLeft;
            _currentTargetTop = targetTop;

            // 设置动画目标值
            _leftAnimation.To = targetLeft;
            _topAnimation.To = targetTop;
            _leftAnimation.Duration = TimeSpan.FromMilliseconds(durationMs);
            _topAnimation.Duration = TimeSpan.FromMilliseconds(durationMs);

            // 停止当前动画
            _storyboard.Stop();

            // 开始新动画
            _isAnimating = true;
            _storyboard.Begin();
        }

        /// <summary>
        /// 显示动画（100ms）
        /// </summary>
        public void AnimateShowDock(double targetLeft, double targetTop)
        {
            ExecuteAnimation(targetLeft, targetTop, 100, false);
        }

        /// <summary>
        /// 隐藏动画（80ms，更快一点）
        /// </summary>
        public void AnimateHideDock(double targetLeft, double targetTop)
        {
            ExecuteAnimation(targetLeft, targetTop, 80, false);
        }

        /// <summary>
        /// 切换边缘动画（150ms）
        /// </summary>
        public void AnimateSwitchEdge(double targetLeft, double targetTop)
        {
            ExecuteAnimation(targetLeft, targetTop, 150, false);
        }

        /// <summary>
        /// 立即停止动画
        /// </summary>
        public void StopCurrentAnimation()
        {
            _storyboard.Stop();
            _isAnimating = false;
            _pendingAction = null;

            // 清除动画效果
            _window.BeginAnimation(Window.LeftProperty, null);
            _window.BeginAnimation(Window.TopProperty, null);
        }

        /// <summary>
        /// 立即移动（无动画）
        /// </summary>
        public void MoveImmediate(double left, double top)
        {
            StopCurrentAnimation();
            _window.Left = left;
            _window.Top = top;
            _currentTargetLeft = left;
            _currentTargetTop = top;
        }
    }
}
