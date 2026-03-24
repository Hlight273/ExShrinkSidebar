using ExShrinkSidebar.Script.Model;
using ExShrinkSidebar.Script.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ExShrinkSidebar.Script.Core
{
    public static class BtnCfgLogicRunner
    {
        /// <summary>
        /// 执行配置逻辑（入口）
        /// </summary>
        public static void Run(ButtonConfig cfg)
        {
            if (cfg == null) return;

            // 【关键修复】使用 Task.Run 在后台线程执行整个逻辑链
            // 这样即使内部有 .GetAwaiter().GetResult() 或耗时操作，也不会阻塞 UI 线程
            Task.Run(() => ExecuteInternal(cfg));
        }

        private static void ExecuteInternal(ButtonConfig cfg)
        {
            try
            {
                var steps = ExpandExecutionSteps(cfg);
                foreach (var step in steps)
                {
                    DoLogic(step);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteInternal Error: {ex.Message}");
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
            if (cfg == null) return;

            if (cfg.configType == ExConfigType.Combine)
            {
                if (cfg.logicChain == null) return;
                foreach (var child in cfg.logicChain)
                {
                    AppendSteps(child, result);
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
            if (string.IsNullOrWhiteSpace(path)) return false;

            var config = arg?.windowCorner != null ? new ExplorerWindowConfig
            {
                Width = arg.windowWidth,
                Height = arg.windowHeight,
                Corner = arg.windowCorner.Value
            } : null;

            // 此时因为在后台线程，同步等待是安全的
            return ExplorerWindowHelper.OpenFolderWithConfigAsync(path, config).GetAwaiter().GetResult();
        }

        private static bool ExecuteTarget(ButtonConfigNodeArg arg)
        {
            var target = arg?.path ?? string.Empty;
            if (string.IsNullOrWhiteSpace(target)) return false;
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
    }
}
