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
    private enum DockVisibilityState
    {
        Hidden,
        Shown,
        AnimatingToHidden,
        AnimatingToShown
    }

    private readonly Window window;
    private IntPtr hwnd = IntPtr.Zero;
    private bool paused = false;

    // 动画相关变量
    private readonly DispatcherTimer animationTimer;

    // 双缓冲变量：动画过程中只读写这些变量，绝不访问 window 属性
    private double animCurrentLeft;
    private double animCurrentTop;
    private double animTargetLeft;
    private double animTargetTop;
    private double cachedWidth;
    private double cachedHeight;
    private DockVisibilityState visibilityState = DockVisibilityState.Hidden;

    private const double ANIMATION_FACTOR = 0.2; // 调大一点，响应更快
    private const double DOCK_THICKNESS_DIP = 150;
    private const double HIDE_PADDING_DIP = 20;
    private const double EDGE_TRIGGER_DIP = 2;

    public DockManager(Window w)
    {
        window = w;
        RegisterTopmostEnforcement();
        InitializeWindowHandle();
        EnsureTopmost();

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

    private void RegisterTopmostEnforcement()
    {
        window.SourceInitialized += OnSourceInitialized;
        window.Deactivated += OnWindowDeactivated;
    }

    private void UnregisterTopmostEnforcement()
    {
        window.SourceInitialized -= OnSourceInitialized;
        window.Deactivated -= OnWindowDeactivated;
    }

    private void OnSourceInitialized(object sender, EventArgs e)
    {
        InitializeWindowHandle();
        EnsureTopmost();
    }

    private void OnWindowDeactivated(object sender, EventArgs e)
    {
        // 窗口失焦后不抢占焦点，在当前消息处理完成后恢复原生置顶层级。
        window.Dispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle,
            new Action(EnsureTopmostWhenVisible));
    }

    private void InitializeWindowHandle()
    {
        if (hwnd == IntPtr.Zero)
        {
            hwnd = new WindowInteropHelper(window).Handle;
        }
    }

    private void EnsureTopmostWhenVisible()
    {
        if (window.IsVisible)
        {
            EnsureTopmost();
        }
    }

    private void EnsureTopmost()
    {
        window.Topmost = true;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
            SWP_TOPMOST_FLAGS);
    }

    private double HiddenOffset
    {
        // 缓存尺寸使用物理像素，与鼠标 Hook 和 Screen.Bounds 保持同一坐标系。
        get => (DockState.curDockOrientation == DockOrientation.Vertical) ? cachedWidth : cachedHeight;
    }

    private double CurrentScreenScale => GetScreenScale(ScreenHelper.GetBounds(DockState.CurrentScreenIndex));

    private static double GetScreenScale(System.Drawing.Rectangle bounds)
    {
        try
        {
            var monitor = MonitorFromPoint(new POINT
            {
                X = bounds.Left + bounds.Width / 2,
                Y = bounds.Top + bounds.Height / 2
            }, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero && GetDpiForMonitor(monitor, MDT_EFFECTIVE_DPI, out var dpiX, out _) == 0)
            {
                return dpiX / 96d;
            }
        }
        catch (DllNotFoundException)
        {
            // Windows 8.0 及以下没有 Shcore.dll，按 100% 缩放回退。
        }

        return 1d;
    }

    private RECT GetWindowBounds()
    {
        GetWindowRect(hwnd, out var bounds);
        return bounds;
    }

    private void ApplyNativePosition(double left, double top, double width, double height)
    {
        SetWindowPos(hwnd, HWND_TOPMOST,
            (int)Math.Round(left),
            (int)Math.Round(top),
            Math.Max(1, (int)Math.Round(width)),
            Math.Max(1, (int)Math.Round(height)),
            SWP_TOPMOST_POSITION_FLAGS);
    }

    private void SyncWpfBoundsFromNative(double left, double top)
    {
        var transform = PresentationSource.FromVisual(window)?.CompositionTarget?.TransformFromDevice;
        if (transform == null)
        {
            return;
        }

        var logicalTopLeft = transform.Value.Transform(new System.Windows.Point(left, top));
        var logicalSize = transform.Value.Transform(new Vector(cachedWidth, cachedHeight));
        window.Left = logicalTopLeft.X;
        window.Top = logicalTopLeft.Y;
        window.Width = logicalSize.X;
        window.Height = logicalSize.Y;
    }

    private void ScheduleWpfBoundsSync(double left, double top)
    {
        window.Dispatcher.BeginInvoke(
            DispatcherPriority.Render,
            new Action(() => SyncWpfBoundsFromNative(left, top)));
    }

    private void EnsureCachedSize()
    {
        if (cachedWidth <= 0)
        {
            var bounds = GetWindowBounds();
            cachedWidth = bounds.Right - bounds.Left;
        }

        if (cachedHeight <= 0)
        {
            var bounds = GetWindowBounds();
            cachedHeight = bounds.Bottom - bounds.Top;
        }
    }

    // 初始化位置 (瞬间定位)
    public void updateEdgePosition()
    {
        animationTimer.Stop(); // 停止正在进行的动画

        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);
        var dockThickness = DOCK_THICKNESS_DIP * CurrentScreenScale;
        double EDGE_LENGTH = (DockState.curDockOrientation == DockOrientation.Vertical
            ? bound.Height : bound.Width) * 0.9;

        double newLeft, newTop;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top:
                cachedWidth = EDGE_LENGTH;
                cachedHeight = dockThickness;
                newLeft = bound.Left + (bound.Width - cachedWidth) / 2;
                newTop = bound.Top - cachedHeight;
                break;
            case DockEdge.Bottom:
                cachedWidth = EDGE_LENGTH;
                cachedHeight = dockThickness;
                newLeft = bound.Left + (bound.Width - cachedWidth) / 2;
                newTop = bound.Bottom;
                break;
            case DockEdge.Left:
                cachedWidth = dockThickness;
                cachedHeight = EDGE_LENGTH;
                newLeft = bound.Left - cachedWidth;
                newTop = bound.Top + (bound.Height - cachedHeight) / 2;
                break;
            case DockEdge.Right:
                cachedWidth = dockThickness;
                cachedHeight = EDGE_LENGTH;
                newLeft = bound.Right;
                newTop = bound.Top + (bound.Height - cachedHeight) / 2;
                break;
            default: return;
        }

        // 1. 同步动画缓冲区变量（统一使用物理像素）
        animCurrentLeft = newLeft;
        animCurrentTop = newTop;
        animTargetLeft = newLeft;
        animTargetTop = newTop;
        visibilityState = DockVisibilityState.Hidden;

        // 2. 使用 Win32 API 瞬间定位，避免 WPF 布局延迟
        ApplyNativePosition(newLeft, newTop, cachedWidth, cachedHeight);
        ScheduleWpfBoundsSync(newLeft, newTop);

        Debug.WriteLine("Edge w h:"+ cachedWidth + " " + cachedHeight);
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
        var scale = CurrentScreenScale;
        var offset = HiddenOffset + HIDE_PADDING_DIP * scale;
        var trigger = EDGE_TRIGGER_DIP * scale;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top:
                if (y <= bound.Top + trigger) ShowDock();
                else if (y > bound.Top + offset) HideDock();
                break;
            case DockEdge.Bottom:
                if (y >= bound.Bottom - trigger) ShowDock();
                else if (y < bound.Bottom - offset) HideDock();
                break;
            case DockEdge.Left:
                if (x <= bound.Left + trigger) ShowDock();
                else if (x > bound.Left + offset) HideDock();
                break;
            case DockEdge.Right:
                if (x >= bound.Right - trigger) ShowDock();
                else if (x < bound.Right - offset) HideDock();
                break;
        }
    }

    public void ShowDock()
    {
        EnsureTopmost();

        // 动画阶段保持使用停靠时确定的固定尺寸，避免透明窗口布局把 ActualSize 挤小。
        EnsureCachedSize();

        if (visibilityState == DockVisibilityState.Shown || visibilityState == DockVisibilityState.AnimatingToShown)
        {
            return;
        }

        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);

        // 每次显隐都以窗口当前物理位置为基准，避免拖拽后回弹到旧缓存坐标。
        var currentBounds = GetWindowBounds();
        animCurrentLeft = currentBounds.Left;
        animCurrentTop = currentBounds.Top;

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
        visibilityState = DockVisibilityState.AnimatingToShown;
        Debug.WriteLine("show:"+cachedHeight+ " " + cachedWidth);
        // 3. 启动动画
        if (!animationTimer.IsEnabled)
        {
            animationTimer.Start();
        }

    }

    public void HideDock()
    {
        EnsureCachedSize();

        if (visibilityState == DockVisibilityState.Hidden || visibilityState == DockVisibilityState.AnimatingToHidden)
        {
            return;
        }

        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);

        // 每次显隐都以窗口当前物理位置为基准，避免拖拽后回弹到旧缓存坐标。
        var currentBounds = GetWindowBounds();
        animCurrentLeft = currentBounds.Left;
        animCurrentTop = currentBounds.Top;

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
        visibilityState = DockVisibilityState.AnimatingToHidden;

        // 3. 启动动画
        if (!animationTimer.IsEnabled)
        {
            animationTimer.Start();
        }
        Debug.WriteLine("hide w d:" + cachedWidth + " " + cachedHeight);
    }

    private void AnimationTimer_Tick(object sender, EventArgs e)
    {
        // 【关键优化点】
        // 这里的逻辑非常纯粹：纯数学计算 + 纯 Win32 API 调用
        // 没有任何 WPF 属性读取，没有布局计算，所以帧率极高

        animCurrentLeft += (animTargetLeft - animCurrentLeft) * ANIMATION_FACTOR;
        animCurrentTop += (animTargetTop - animCurrentTop) * ANIMATION_FACTOR;

        Debug.WriteLine("tick:" + cachedWidth + " "+ cachedHeight + " ;"+ animCurrentLeft + " " + animCurrentTop);

        // 直接调用 Win32 API 更新窗口位置
        ApplyNativePosition(animCurrentLeft, animCurrentTop, cachedWidth, cachedHeight);

        // 结束判定
        if (Math.Abs(animCurrentLeft - animTargetLeft) < 1.0 &&
            Math.Abs(animCurrentTop - animTargetTop) < 1.0)
        {
            ApplyNativePosition(animTargetLeft, animTargetTop, cachedWidth, cachedHeight);

            // 动画结束时，将物理像素坐标同步回 WPF 的逻辑坐标。
            ScheduleWpfBoundsSync(animTargetLeft, animTargetTop);
            visibilityState = visibilityState == DockVisibilityState.AnimatingToShown
                ? DockVisibilityState.Shown
                : DockVisibilityState.Hidden;

            animationTimer.Stop();
        }
    }

    public void Cleanup()
    {
        animationTimer.Stop();
        HookManager.MouseMoved -= OnMouseMove;
        HookManager.Stop();
        UnregisterTopmostEnforcement();
    }

    #region Win32 API
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("Shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hmonitor, uint dpiType, out uint dpiX, out uint dpiY);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_NOOWNERZORDER = 0x0200;
    private const uint SWP_NOSENDCHANGING = 0x0400; // 防止 WPF 收到多余的移动消息
    private const uint SWP_TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOOWNERZORDER;
    private const uint SWP_TOPMOST_POSITION_FLAGS = SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSENDCHANGING;
    private const uint MONITOR_DEFAULTTONEAREST = 2;
    private const uint MDT_EFFECTIVE_DPI = 0;
    #endregion
}
