using ExShrinkSidebar.Script.Model;
using ExShrinkSidebar.Script.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ExShrinkSidebar.Script.Core
{
    public static class BtnCfgLogicRunner
    {
        public static void Run(ButtonConfig cfg)
        {
            if (cfg == null)
            {
                return;
            }

            var steps = ExpandExecutionSteps(cfg);
            foreach (var step in steps)
            {
                DoLogic(step);
            }
        }

        public static IReadOnlyList<ButtonConfig> ExpandExecutionSteps(ButtonConfig cfg)
        {
            var result = new List<ButtonConfig>();
            AppendSteps(cfg, result);
            return result;
        }

        private static void AppendSteps(ButtonConfig cfg, List<ButtonConfig> result)
        {
            if (cfg == null)
            {
                return;
            }

            if (cfg.configType == ExConfigType.Combine)
            {
                if (cfg.logicChain == null)
                {
                    return;
                }

                for (int i = 0; i < cfg.logicChain.Count; i++)
                {
                    AppendSteps(cfg.logicChain[i], result);
                }

                return;
            }

            result.Add(cfg);
        }

        private static bool DoLogic(ButtonConfig cfg)
        {
            switch (cfg.configType)
            {
                case ExConfigType.Notepad:
                    return StartShellTarget("notepad.exe", string.Empty);
                case ExConfigType.OpenFolder:
                    return OpenFolder(cfg.arg);
                case ExConfigType.Execute:
                    return ExecuteTarget(cfg.arg);
            }

            return true;
        }

        private static bool OpenFolder(ButtonConfigNodeArg arg)
        {
            var path = arg?.path ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var success = StartShellTarget("explorer.exe", QuoteArgument(path));
            if (success && arg?.windowCorner != null)
            {
                _ = Task.Run(async () => await TryRepositionExplorerWindowAsync(arg));
            }

            return success;
        }

        private static bool ExecuteTarget(ButtonConfigNodeArg arg)
        {
            var target = arg?.path ?? string.Empty;
            if (string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            return StartShellTarget(target, arg?.arguments ?? string.Empty);
        }

        private static bool StartShellTarget(string fileName, string arguments)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments ?? string.Empty,
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StartShellTarget " + ex.Message);
                return false;
            }
        }

        private static async Task TryRepositionExplorerWindowAsync(ButtonConfigNodeArg arg)
        {
            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(250);
                var hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero || !IsExplorerWindow(hwnd))
                {
                    continue;
                }

                if (!GetWindowRect(hwnd, out RECT currentRect))
                {
                    continue;
                }

                var bounds = ScreenHelper.GetBounds(0);
                var targetWidth = arg.windowWidth > 0 ? arg.windowWidth : bounds.Width / 2;
                var targetHeight = arg.windowHeight > 0 ? arg.windowHeight : bounds.Height / 2;

                targetWidth = Math.Min(targetWidth, bounds.Width);
                targetHeight = Math.Min(targetHeight, bounds.Height);

                var targetLeft = bounds.Left;
                var targetTop = bounds.Top;

                switch (arg.windowCorner)
                {
                    case ExWindowCorner.TopRight:
                        targetLeft = bounds.Right - targetWidth;
                        break;
                    case ExWindowCorner.BottomLeft:
                        targetTop = bounds.Bottom - targetHeight;
                        break;
                    case ExWindowCorner.BottomRight:
                        targetLeft = bounds.Right - targetWidth;
                        targetTop = bounds.Bottom - targetHeight;
                        break;
                }

                SetWindowPos(hwnd, IntPtr.Zero, targetLeft, targetTop, targetWidth, targetHeight, SWP_NOZORDER | SWP_SHOWWINDOW);
                return;
            }
        }

        private static bool IsExplorerWindow(IntPtr hwnd)
        {
            var className = new StringBuilder(256);
            GetClassName(hwnd, className, className.Capacity);
            var windowClass = className.ToString();
            return string.Equals(windowClass, "CabinetWClass", StringComparison.OrdinalIgnoreCase)
                || string.Equals(windowClass, "ExploreWClass", StringComparison.OrdinalIgnoreCase);
        }

        private static string QuoteArgument(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return "\"" + value.Trim().Replace("\"", "\\\"") + "\"";
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_SHOWWINDOW = 0x0040;

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
