using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
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
    private double cachedWidth;
    private double cachedHeight;
    private DockVisibilityState visibilityState = DockVisibilityState.Hidden;

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

    private Matrix TransformFromDevice
    {
        get
        {
            var source = PresentationSource.FromVisual(window);
            return source?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
        }
    }

    private Matrix TransformToDevice
    {
        get
        {
            var source = PresentationSource.FromVisual(window);
            return source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
        }
    }

    private System.Windows.Point PixelsToDip(double x, double y) => TransformFromDevice.Transform(new System.Windows.Point(x, y));

    private Rect ScreenBoundsToDip(System.Drawing.Rectangle bounds)
    {
        var topLeft = PixelsToDip(bounds.Left, bounds.Top);
        var bottomRight = PixelsToDip(bounds.Right, bounds.Bottom);
        return new Rect(topLeft, bottomRight);
    }

    private void ApplyNativePosition(double left, double top, double width, double height)
    {
        var topLeft = TransformToDevice.Transform(new System.Windows.Point(left, top));
        var bottomRight = TransformToDevice.Transform(new System.Windows.Point(left + width, top + height));
        var pixelWidth = Math.Max(1, (int)Math.Round(bottomRight.X - topLeft.X));
        var pixelHeight = Math.Max(1, (int)Math.Round(bottomRight.Y - topLeft.Y));

        SetWindowPos(hwnd, IntPtr.Zero,
            (int)Math.Round(topLeft.X),
            (int)Math.Round(topLeft.Y),
            pixelWidth,
            pixelHeight,
            SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
    }

    private void EnsureCachedSize()
    {
        if (cachedWidth <= 0)
        {
            cachedWidth = window.Width;
        }

        if (cachedHeight <= 0)
        {
            cachedHeight = window.Height;
        }
    }

    // 初始化位置 (瞬间定位)
    public void updateEdgePosition()
    {
        animationTimer.Stop(); // 停止正在进行的动画

        var bound = ScreenBoundsToDip(ScreenHelper.GetBounds(DockState.CurrentScreenIndex));
        const double DOCK_SIZE = 150;
        double EDGE_LENGTH = (DockState.curDockOrientation == DockOrientation.Vertical
            ? bound.Height : bound.Width) * 0.9;

        double newLeft, newTop;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top:
                cachedWidth = EDGE_LENGTH;
                cachedHeight = DOCK_SIZE;
                newLeft = bound.Left + (bound.Width - cachedWidth) / 2;
                newTop = bound.Top - cachedHeight;
                break;
            case DockEdge.Bottom:
                cachedWidth = EDGE_LENGTH;
                cachedHeight = DOCK_SIZE;
                newLeft = bound.Left + (bound.Width - cachedWidth) / 2;
                newTop = bound.Bottom;
                break;
            case DockEdge.Left:
                cachedWidth = DOCK_SIZE;
                cachedHeight = EDGE_LENGTH;
                newLeft = bound.Left - cachedWidth;
                newTop = bound.Top + (bound.Height - cachedHeight) / 2;
                break;
            case DockEdge.Right:
                cachedWidth = DOCK_SIZE;
                cachedHeight = EDGE_LENGTH;
                newLeft = bound.Right;
                newTop = bound.Top + (bound.Height - cachedHeight) / 2;
                break;
            default: return;
        }

        // 1. 先更新内部尺寸状态
        window.Width = cachedWidth;
        window.Height = cachedHeight;

        // 2. 同步动画缓冲区变量
        animCurrentLeft = newLeft;
        animCurrentTop = newTop;
        animTargetLeft = newLeft;
        animTargetTop = newTop;
        visibilityState = DockVisibilityState.Hidden;

        
        // 3. 使用 Win32 API 瞬间定位，避免 WPF 布局延迟
        ApplyNativePosition(newLeft, newTop, cachedWidth, cachedHeight);

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

        var mouse = PixelsToDip(x, y);
        var bound = ScreenBoundsToDip(ScreenHelper.GetBounds(DockState.CurrentScreenIndex));
        var offset = HiddenOffset + 20;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top:
                if (mouse.Y <= bound.Top + 2) ShowDock();
                else if (mouse.Y > bound.Top + offset) HideDock();
                break;
            case DockEdge.Bottom:
                if (mouse.Y >= bound.Bottom - 2) ShowDock();
                else if (mouse.Y < bound.Bottom - offset) HideDock();
                break;
            case DockEdge.Left:
                if (mouse.X <= bound.Left + 2) ShowDock();
                else if (mouse.X > bound.Left + offset) HideDock();
                break;
            case DockEdge.Right:
                if (mouse.X >= bound.Right - 2) ShowDock();
                else if (mouse.X < bound.Right - offset) HideDock();
                break;
        }
    }

    public void ShowDock()
    {
        // 动画阶段保持使用停靠时确定的固定尺寸，避免透明窗口布局把 ActualSize 挤小。
        EnsureCachedSize();

        if (visibilityState == DockVisibilityState.Shown || visibilityState == DockVisibilityState.AnimatingToShown)
        {
            return;
        }

        var bound = ScreenBoundsToDip(ScreenHelper.GetBounds(DockState.CurrentScreenIndex));

        // 每次显隐都以窗口当前真实位置为基准，避免拖拽后回弹到旧缓存坐标。
        animCurrentLeft = window.Left;
        animCurrentTop = window.Top;

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

        var bound = ScreenBoundsToDip(ScreenHelper.GetBounds(DockState.CurrentScreenIndex));

        // 每次显隐都以窗口当前真实位置为基准，避免拖拽后回弹到旧缓存坐标。
        animCurrentLeft = window.Left;
        animCurrentTop = window.Top;

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

            // 动画结束时，将最终位置同步回 WPF (可选，保持状态一致性)
            // 注意：这里是在 Timer 停止前赋值，是安全的
            window.Left = animTargetLeft;
            window.Top = animTargetTop;
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
