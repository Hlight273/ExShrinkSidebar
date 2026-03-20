using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Utils;

public class DockManager
{
    private Window window;
    private IntPtr hwnd = IntPtr.Zero;
    private bool paused = false;

    // 动画相关变量
    private DispatcherTimer animationTimer;

    // 双缓冲变量：动画过程中只读写这些变量，绝不访问 window 属性
    private double animCurrentLeft;
    private double animCurrentTop;
    private double animTargetLeft;
    private double animTargetTop;
    private int cachedWidth;
    private int cachedHeight;

    private const double ANIMATION_FACTOR = 0.2; // 调大一点，响应更快

    public DockManager(Window w)
    {
        window = w;
        window.SourceInitialized += (s, e) =>
        {
            hwnd = new WindowInteropHelper(window).Handle;
        };
        if (hwnd == IntPtr.Zero)
        {
            hwnd = new WindowInteropHelper(window).Handle;
        }

        // 使用 Send 优先级，确保动画逻辑优先于其他 UI 逻辑
        animationTimer = new DispatcherTimer(DispatcherPriority.Send);
        animationTimer.Interval = TimeSpan.FromMilliseconds(1); // 尽可能快的间隔
        animationTimer.Tick += AnimationTimer_Tick;

        Debug.WriteLine("DockManager 初始化");
        HookManager.MouseMoved += OnMouseMove;
        HookManager.Start();
    }

    public void Pause() => paused = true;
    public void Resume() => paused = false;

    private double HiddenOffset
    {
        // 使用缓存尺寸，避免在鼠标 Hook 高频触发时读取 WPF 属性
        get => (DockState.curDockOrientation == DockOrientation.Vertical) ? cachedWidth : cachedHeight;
    }

    // 初始化位置 (瞬间定位)
    public void updateEdgePosition()
    {
        animationTimer.Stop(); // 停止正在进行的动画

        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);
        const double DOCK_SIZE = 150;
        double EDGE_LENGTH = (DockState.curDockOrientation == DockOrientation.Vertical
            ? bound.Height : bound.Width) * 0.9;

        int newLeft, newTop;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top:
                cachedWidth = (int)EDGE_LENGTH;
                cachedHeight = (int)DOCK_SIZE;
                newLeft = (int)(bound.Left + (bound.Width - cachedWidth) / 2);
                newTop = (int)(bound.Top - cachedHeight);
                break;
            case DockEdge.Bottom:
                cachedWidth = (int)EDGE_LENGTH;
                cachedHeight = (int)DOCK_SIZE;
                newLeft = (int)(bound.Left + (bound.Width - cachedWidth) / 2);
                newTop = (int)(bound.Bottom);
                break;
            case DockEdge.Left:
                cachedWidth = (int)DOCK_SIZE;
                cachedHeight = (int)EDGE_LENGTH;
                newLeft = (int)(bound.Left - cachedWidth);
                newTop = (int)(bound.Top + (bound.Height - cachedHeight) / 2);
                break;
            case DockEdge.Right:
                cachedWidth = (int)DOCK_SIZE;
                cachedHeight = (int)EDGE_LENGTH;
                newLeft = (int)(bound.Right);
                newTop = (int)(bound.Top + (bound.Height - cachedHeight) / 2);
                break;
            default: return;
        }

        // 1. 先更新内部尺寸状态
        window.Width = cachedWidth;
        window.Height = cachedHeight;

        // 2. 同步动画缓冲区变量
        animCurrentLeft = newLeft;
        animCurrentTop = newTop;

        // 3. 使用 Win32 API 瞬间定位，避免 WPF 布局延迟
        SetWindowPos(hwnd, IntPtr.Zero, newLeft, newTop, cachedWidth, cachedHeight, SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
    }

    private void OnMouseMove(int x, int y)
    {
        if (paused || hwnd == IntPtr.Zero) return;

        if (!ScreenHelper.IsPointOnScreen(x, y, DockState.CurrentScreenIndex))
        {
            HideDock();
            return;
        }

        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);
        var offset = HiddenOffset + 20;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top:
                if (y <= bound.Top + 2) ShowDock();
                else if (y > bound.Top + offset) HideDock();
                break;
            case DockEdge.Bottom:
                if (y >= bound.Bottom - 2) ShowDock();
                else if (y < bound.Bottom - offset) HideDock();
                break;
            case DockEdge.Left:
                if (x <= bound.Left + 2) ShowDock();
                else if (x > bound.Left + offset) HideDock();
                break;
            case DockEdge.Right:
                if (x >= bound.Right - 2) ShowDock();
                else if (x < bound.Right - offset) HideDock();
                break;
        }
    }

    public void ShowDock()
    {
        // 1. 在动画开始前，读取一次实际尺寸，存入缓存
        // 此时读取是安全的，因为我们还没有开始高频循环
        cachedWidth = (int)window.ActualWidth;
        cachedHeight = (int)window.ActualHeight;

        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);

        // 2. 计算目标位置
        animTargetLeft = animCurrentLeft;
        animTargetTop = animCurrentTop;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top: animTargetTop = bound.Top; break;
            case DockEdge.Bottom: animTargetTop = bound.Bottom - cachedHeight; break;
            case DockEdge.Left: animTargetLeft = bound.Left; break;
            case DockEdge.Right: animTargetLeft = bound.Right - cachedWidth; break;
        }

        // 3. 启动动画
        if (!animationTimer.IsEnabled)
        {
            // 从当前位置开始
            animCurrentLeft = window.Left;
            animCurrentTop = window.Top;
            animationTimer.Start();
        }
    }

    public void HideDock()
    {
        // 1. 读取一次实际尺寸
        cachedWidth = (int)window.ActualWidth;
        cachedHeight = (int)window.ActualHeight;

        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);

        // 2. 计算目标位置
        animTargetLeft = animCurrentLeft;
        animTargetTop = animCurrentTop;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top: animTargetTop = bound.Top - cachedHeight; break;
            case DockEdge.Bottom: animTargetTop = bound.Bottom; break;
            case DockEdge.Left: animTargetLeft = bound.Left - cachedWidth; break;
            case DockEdge.Right: animTargetLeft = bound.Right; break;
        }

        // 3. 启动动画
        if (!animationTimer.IsEnabled)
        {
            animCurrentLeft = window.Left;
            animCurrentTop = window.Top;
            animationTimer.Start();
        }
    }

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        // 【关键优化点】
        // 这里的逻辑非常纯粹：纯数学计算 + 纯 Win32 API 调用
        // 没有任何 WPF 属性读取，没有布局计算，所以帧率极高

        animCurrentLeft += (animTargetLeft - animCurrentLeft) * ANIMATION_FACTOR;
        animCurrentTop += (animTargetTop - animCurrentTop) * ANIMATION_FACTOR;

        // 直接调用 Win32 API 更新窗口位置
        SetWindowPos(hwnd, IntPtr.Zero,
            (int)animCurrentLeft, (int)animCurrentTop,
            cachedWidth, cachedHeight,
            SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOSENDCHANGING);

        // 结束判定
        if (Math.Abs(animCurrentLeft - animTargetLeft) < 1.0 &&
            Math.Abs(animCurrentTop - animTargetTop) < 1.0)
        {
            SetWindowPos(hwnd, IntPtr.Zero,
                (int)animTargetLeft, (int)animTargetTop,
                cachedWidth, cachedHeight,
                SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOSENDCHANGING);

            // 动画结束时，将最终位置同步回 WPF (可选，保持状态一致性)
            // 注意：这里是在 Timer 停止前赋值，是安全的
            window.Left = animTargetLeft;
            window.Top = animTargetTop;

            animationTimer.Stop();
        }
    }

    public void Cleanup()
    {
        animationTimer.Stop();
        HookManager.MouseMoved -= OnMouseMove;
        HookManager.Stop();
    }

    #region Win32 API
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_NOSENDCHANGING = 0x0400; // 防止 WPF 收到多余的移动消息
    #endregion
}
