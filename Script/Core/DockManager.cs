using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Utils;

public class DockManager
{
    private Window window;
    private IntPtr hwnd = IntPtr.Zero;
    private bool paused = false;

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

        Debug.WriteLine("DockManager 初始化");
        HookManager.MouseMoved += OnMouseMove;
        HookManager.Start();
    }

    public void Pause() => paused = true;
    public void Resume() => paused = false;

    private double HiddenOffset
    {
        get => (DockState.curDockOrientation == DockOrientation.Vertical) ? window.Width : window.Height;
    }

    private void UpdateWindowPosition(int left, int top, int width, int height)
    {
        if (hwnd == IntPtr.Zero) return;
        SetWindowPos(hwnd, IntPtr.Zero, left, top, width, height, SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
    }

    public void updateEdgePosition()
    {
        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);
        const double DOCK_SIZE = 150;
        double EDGE_LENGTH = (DockState.curDockOrientation == DockOrientation.Vertical
            ? bound.Height : bound.Width) * 0.9;

        int newLeft, newTop, newWidth, newHeight;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top:
                newWidth = (int)EDGE_LENGTH; newHeight = (int)DOCK_SIZE;
                newLeft = (int)(bound.Left + (bound.Width - EDGE_LENGTH) / 2);
                newTop = (int)(bound.Top - DOCK_SIZE);
                break;
            case DockEdge.Bottom:
                newWidth = (int)EDGE_LENGTH; newHeight = (int)DOCK_SIZE;
                newLeft = (int)(bound.Left + (bound.Width - EDGE_LENGTH) / 2);
                newTop = (int)(bound.Bottom);
                break;
            case DockEdge.Left:
                newWidth = (int)DOCK_SIZE; newHeight = (int)EDGE_LENGTH;
                newLeft = (int)(bound.Left - DOCK_SIZE);
                newTop = (int)(bound.Top + (bound.Height - EDGE_LENGTH) / 2);
                break;
            case DockEdge.Right:
                newWidth = (int)DOCK_SIZE; newHeight = (int)EDGE_LENGTH;
                newLeft = (int)(bound.Right);
                newTop = (int)(bound.Top + (bound.Height - EDGE_LENGTH) / 2);
                break;
            default: return;
        }

        UpdateWindowPosition(newLeft, newTop, newWidth, newHeight);
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
        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);
        int targetLeft = (int)window.Left;
        int targetTop = (int)window.Top;
        int currentWidth = (int)window.ActualWidth;
        int currentHeight = (int)window.ActualHeight;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top: targetTop = (int)bound.Top; break;
            case DockEdge.Bottom: targetTop = (int)(bound.Bottom - HiddenOffset); break;
            case DockEdge.Left: targetLeft = (int)bound.Left; break;
            case DockEdge.Right: targetLeft = (int)(bound.Right - HiddenOffset); break;
        }

        UpdateWindowPosition(targetLeft, targetTop, currentWidth, currentHeight);
    }

    public void HideDock()
    {
        var bound = ScreenHelper.GetBounds(DockState.CurrentScreenIndex);
        int targetLeft = (int)window.Left;
        int targetTop = (int)window.Top;
        int currentWidth = (int)window.ActualWidth;
        int currentHeight = (int)window.ActualHeight;

        switch (DockState.CurrentEdge)
        {
            case DockEdge.Top: targetTop = (int)(bound.Top - HiddenOffset); break;
            case DockEdge.Bottom: targetTop = (int)bound.Bottom; break;
            case DockEdge.Left: targetLeft = (int)(bound.Left - HiddenOffset); break;
            case DockEdge.Right: targetLeft = (int)bound.Right; break;
        }

        UpdateWindowPosition(targetLeft, targetTop, currentWidth, currentHeight);
    }

    public void Cleanup()
    {
        HookManager.MouseMoved -= OnMouseMove;
        HookManager.Stop();
    }

    #region Win32 API
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_NOSENDCHANGING = 0x0400;
    #endregion
}
