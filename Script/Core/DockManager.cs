using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Utils;
using ExShrinkSidebar.UI.Views.MainWindow;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Threading; // 只加这个命名空间
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class DockManager
{
    private System.Windows.Window window;
    private WindowAnimator animator;
    bool paused = false;

    // 只加防抖相关的字段
    private DispatcherTimer _debounceTimer;
    private int _lastX, _lastY;
    private bool _hasPendingMove = false;

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    private double HiddenOffset
    {
        get => (DockState.curDockOrientation == DockOrientation.Vertical) ? window.Width : window.Height;
    }

    public DockManager(System.Windows.Window w, WindowAnimator a)
    {
        window = w;
        animator = a;

        // 只加防抖定时器初始化
        _debounceTimer = new DispatcherTimer();
        _debounceTimer.Interval = TimeSpan.FromMilliseconds(30);
        _debounceTimer.Tick += OnDebounceTimerTick;

        Debug.WriteLine("DockManager 初始化");
        HookManager.MouseMoved += OnMouseMove;
        HookManager.Start();
    }

    public void updateEdgePosition()
    {
        var edge = DockState.CurrentEdge;
        DockState.SetEdge(edge);

        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);
        var width = 200;

        switch (edge)
        {
            case DockEdge.Top:
                window.Left = bound.Left;
                window.Width = bound.Width / 2;
                window.Top = bound.Top - HiddenOffset;
                window.Height = width;
                break;

            case DockEdge.Bottom:
                window.Left = bound.Left;
                window.Width = bound.Width / 2;
                window.Top = bound.Bottom;
                window.Height = width;
                break;

            case DockEdge.Left:
                window.Top = bound.Top;
                window.Height = bound.Height / 2;
                window.Left = bound.Left - HiddenOffset;
                window.Width = width;
                break;

            case DockEdge.Right:
                window.Top = bound.Top;
                window.Height = bound.Height / 2;
                window.Left = bound.Right;
                window.Width = width;
                break;
        }
    }

    // 只改这个方法：记录鼠标位置，启动定时器
    private void OnMouseMove(int x, int y)
    {
        if (paused) return;

        _lastX = x;
        _lastY = y;
        _hasPendingMove = true;

        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    // 新增：防抖定时器处理
    private void OnDebounceTimerTick(object sender, EventArgs e)
    {
        _debounceTimer.Stop();

        if (paused) return;
        if (!_hasPendingMove) return;

        _hasPendingMove = false;

        // 执行原来的逻辑
        if (!ScreenHelper.IsPointOnScreen(_lastX, _lastY, DockState.CurrentScreenIndex))
        {
            HideDock();
            return;
        }

        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);
        var leaveOffset = HiddenOffset + 20;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top:
                if (_lastY <= bound.Top + 2) ShowDock();
                else if (_lastY > bound.Top + leaveOffset) HideDock();
                break;

            case DockEdge.Bottom:
                if (_lastY >= bound.Bottom - 2) ShowDock();
                else if (_lastY < bound.Bottom - leaveOffset) HideDock();
                break;

            case DockEdge.Left:
                if (_lastX <= bound.Left + 2) ShowDock();
                else if (_lastX > bound.Left + leaveOffset) HideDock();
                break;

            case DockEdge.Right:
                if (_lastX >= bound.Right - 2) ShowDock();
                else if (_lastX < bound.Right - leaveOffset) HideDock();
                break;
        }
    }

    // ShowDock 和 HideDock 完全不变
    private void ShowDock()
    {
        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);

        Debug.WriteLine("显示 Dock");

        double targetLeft = window.Left;
        double targetTop = window.Top;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top:
                targetTop = bound.Top;
                break;
            case DockEdge.Bottom:
                targetTop = bound.Bottom - HiddenOffset;
                break;
            case DockEdge.Left:
                targetLeft = bound.Left;
                break;
            case DockEdge.Right:
                targetLeft = bound.Right - HiddenOffset;
                break;
        }
        animator.AnimateShowDock(targetLeft, targetTop);
    }

    private void HideDock()
    {
        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);

        Debug.WriteLine("隐藏 Dock");

        double targetLeft = window.Left;
        double targetTop = window.Top;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top:
                targetTop = bound.Top - HiddenOffset;
                break;
            case DockEdge.Bottom:
                targetTop = bound.Bottom;
                break;
            case DockEdge.Left:
                targetLeft = bound.Left - HiddenOffset;
                break;
            case DockEdge.Right:
                targetLeft = bound.Right;
                break;
        }
        animator.AnimateHideDock(targetLeft, targetTop);
    }

    // 加个清理方法（可选）
    public void Cleanup()
    {
        if (_debounceTimer != null)
        {
            _debounceTimer.Stop();
            _debounceTimer.Tick -= OnDebounceTimerTick;
        }
        HookManager.MouseMoved -= OnMouseMove;
    }
}