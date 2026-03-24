using ExShrinkSidebar.Script.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
// 引入 COM 组件命名空间
using SHDocVw;

namespace ExShrinkSidebar.Script.Utils
{
    public static class ExplorerWindowHelper
    {
        /// <summary>
        /// 打开文件夹并应用窗口配置（优先复用已有窗口）
        /// </summary>
        public static async Task<bool> OpenFolderWithConfigAsync(string folderPath, ExplorerWindowConfig config)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) return false;

            // 1. 规范化路径
            string normalizedPath;
            try
            {
                normalizedPath = Path.GetFullPath(folderPath.Trim().TrimEnd('\\'));
            }
            catch
            {
                normalizedPath = folderPath;
            }

            // 2. 尝试通过 COM 查找现有窗口
            // 使用 Task.Run 避免 COM 操作在某些情况下可能造成的短暂卡顿
            var existingHwnd = await Task.Run(() => FindExplorerWindowByPath(normalizedPath));

            if (existingHwnd != IntPtr.Zero)
            {
                // 3. 找到了 -> 激活并配置
                return ActivateAndConfigure(existingHwnd, config);
            }
            else
            {
                // 4. 没找到 -> 打开新的并配置
                return await OpenAndConfigureNewWindowAsync(normalizedPath, config);
            }
        }

        /// <summary>
        /// 使用 SHDocVw (ShellWindows) 精确查找路径对应的窗口句柄
        /// </summary>
        private static IntPtr FindExplorerWindowByPath(string targetPath)
        {
            try
            {
                // ShellWindows 代表当前运行的所有 Shell 窗口（包括资源管理器和 IE）
                var shellWindows = new ShellWindows();

                foreach (InternetExplorer window in shellWindows)
                {
                    try
                    {
                        // 过滤：只处理资源管理器窗口
                        // 资源管理器的进程名通常为 "explorer"，而 IE 是 "iexplore"
                        string filename = Path.GetFileNameWithoutExtension(window.FullName);
                        if (filename == null || !filename.Equals("explorer", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // 获取窗口当前路径
                        // LocationURL 格式通常是 "file:///C:/Users/..."，需要转换为本地路径
                        string url = window.LocationURL;
                        if (string.IsNullOrEmpty(url)) continue;

                        string localPath = Uri.UnescapeDataString(url);
                        if (Uri.TryCreate(localPath, UriKind.Absolute, out var uri))
                        {
                            string explorerPath = uri.LocalPath;

                            // 比较路径（忽略大小写，忽略末尾反斜杠）
                            if (string.Equals(explorerPath.TrimEnd('\\'), targetPath.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                            {
                                // 匹配成功，返回窗口句柄
                                return (IntPtr)window.HWND;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"检查 Shell 窗口时出错: {ex.Message}");
                    }
                    finally
                    {
                        // 释放 COM 对象引用，防止内存泄漏
                        if (window != null) Marshal.ReleaseComObject(window);
                    }
                }

                // 释放集合对象
                if (shellWindows != null) Marshal.ReleaseComObject(shellWindows);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FindExplorerWindowByPath Error: {ex.Message}");
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// 激活窗口并配置位置
        /// </summary>
        private static bool ActivateAndConfigure(IntPtr hwnd, ExplorerWindowConfig config)
        {
            try
            {
                // 如果最小化，先恢复
                if (IsIconic(hwnd))
                {
                    ShowWindow(hwnd, SW_RESTORE);
                }

                SetForegroundWindow(hwnd);

                if (config != null)
                {
                    // 稍微延时，确保窗口状态更新
                    System.Threading.Thread.Sleep(50);
                    ConfigureWindowPosition(hwnd, config);
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 打开新窗口并配置
        /// </summary>
        private static async Task<bool> OpenAndConfigureNewWindowAsync(string path, ExplorerWindowConfig config)
        {
            // 记录打开前的窗口列表
            var beforeWindows = GetExplorerWindowHandles();

            // 启动新进程
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = QuoteArgument(path),
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OpenExplorer Error: " + ex.Message);
                return false;
            }

            // 如果不需要配置，直接返回
            if (config == null) return true;

            // 轮询查找新出现的窗口
            return await Task.Run(() =>
            {
                IntPtr targetHwnd = IntPtr.Zero;
                for (int i = 0; i < 20; i++) // 尝试约5秒
                {
                    System.Threading.Thread.Sleep(250);

                    var afterWindows = GetExplorerWindowHandles();

                    // 找出新增的那个窗口
                    targetHwnd = afterWindows.FirstOrDefault(h => !beforeWindows.Contains(h));

                    if (targetHwnd != IntPtr.Zero)
                        break;
                }

                if (targetHwnd != IntPtr.Zero)
                {
                    ConfigureWindowPosition(targetHwnd, config);
                    return true;
                }
                return false;
            });
        }

        private static HashSet<IntPtr> GetExplorerWindowHandles()
        {
            var handles = new HashSet<IntPtr>();
            EnumWindows((hwnd, lParam) =>
            {
                if (IsExplorerWindow(hwnd))
                {
                    handles.Add(hwnd);
                }
                return true;
            }, IntPtr.Zero);
            return handles;
        }

        private static void ConfigureWindowPosition(IntPtr hwnd, ExplorerWindowConfig config)
        {
            if (config == null) return;

            // 获取屏幕尺寸
            var bounds = ScreenHelper.GetBounds(0);
            if (bounds == null) return;

            int targetWidth = config.Width > 0 ? config.Width : bounds.Width / 2;
            int targetHeight = config.Height > 0 ? config.Height : bounds.Height / 2;

            targetWidth = Math.Min(targetWidth, bounds.Width);
            targetHeight = Math.Min(targetHeight, bounds.Height);

            int targetLeft = bounds.Left;
            int targetTop = bounds.Top;

            switch (config.Corner)
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
        }

        private static bool IsExplorerWindow(IntPtr hwnd)
        {
            var className = new StringBuilder(256);
            GetClassName(hwnd, className, className.Capacity);
            var cls = className.ToString();
            return cls == "CabinetWClass" || cls == "ExploreWClass";
        }

        private static string QuoteArgument(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return "\"" + value.Trim().Replace("\"", "\\\"") + "\"";
        }

        #region P/Invoke

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int SW_RESTORE = 9;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_SHOWWINDOW = 0x0040;

        #endregion
    }

    public class ExplorerWindowConfig
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public ExWindowCorner Corner { get; set; }
    }
}
