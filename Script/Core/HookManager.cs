using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

public static class HookManager
{
    public static event Action<int, int> MouseMoved;

    private const int WH_MOUSE_LL = 14;
    private const int WM_MOUSEMOVE = 0x0200;

    private static IntPtr hookId = IntPtr.Zero;

    private static LowLevelMouseProc proc = HookCallback;

    public static void Start()
    {
        if (hookId != IntPtr.Zero)
            return;

        Debug.WriteLine("安装鼠标Hook");

        hookId = SetWindowsHookEx(
            WH_MOUSE_LL,
            proc,
            GetModuleHandle(null),
            0);

        Debug.WriteLine($"HookID: {hookId}");
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
        {
            var data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

            int x = data.pt.X;
            int y = data.pt.Y;
            var app = System.Windows.Application.Current;
            var dispatcher = app?.Dispatcher;
            if (hookId != IntPtr.Zero &&
                dispatcher != null &&
                !dispatcher.HasShutdownStarted &&
                !dispatcher.HasShutdownFinished)
            {
                dispatcher.BeginInvoke(() =>
                {
                    if (hookId != IntPtr.Zero)
                    {
                        MouseMoved?.Invoke(x, y);
                    }
                });
            }
        }

        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }

    /// <summary>
    /// 停止鼠标Hook
    /// </summary>
    public static void Stop()
    {
        if (hookId == IntPtr.Zero)
            return;

        Debug.WriteLine("卸载鼠标Hook");

        UnhookWindowsHookEx(hookId);
        hookId = IntPtr.Zero;
    }

    #region Win32

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(
        int idHook,
        LowLevelMouseProc lpfn,
        IntPtr hMod,
        uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk,
        int nCode,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    #endregion
}
