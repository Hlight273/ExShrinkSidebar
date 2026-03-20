using System;
using System.IO;

namespace ExShrinkSidebar.Script.Utils
{
    public static class ResHelper
    {
        /// <summary>
        /// 程序根目录
        /// </summary>
        public static readonly string BaseDir =
            AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 获取资源绝对路径
        /// 输入统一使用 "/" 分隔
        /// </summary>
        public static string GetPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return BaseDir;

            // 统一斜杠
            relativePath = relativePath.Replace('\\', '/');

            // 分割路径
            var parts = relativePath.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

            // Combine
            var path = Path.Combine(parts);

            return Path.Combine(BaseDir, path);
        }
    }
}