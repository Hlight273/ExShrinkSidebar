using ExShrinkSidebar.Script.Model;
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

        private const string IconFolderPrefix = "Asset/Resource/Icons";

        /// <summary>
        /// 获取资源绝对路径
        /// </summary>
        /// <param name="relativePath">相对路径（输入统一使用 "/" 分隔）</param>
        /// <param name="prefix">路径前缀（可选），例如 "Asset/Resource/Icons"</param>
        public static string GetPath(string relativePath, string prefix = null)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                prefix = prefix.Replace('\\', '/').Trim('/'); //// 如果有前缀，将其与相对路径拼接 统一斜杠并去除两端空白
                relativePath = $"{prefix}/{relativePath}";
            }

            if (string.IsNullOrWhiteSpace(relativePath))
                return BaseDir;

            relativePath = relativePath.Replace('\\', '/'); //统一斜杠
            var parts = relativePath.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);
            var path = Path.Combine(parts);
            return Path.Combine(BaseDir, path);
        }


        /// <summary>
        /// 获取扩展配置类型图标路径
        /// </summary>
        public static string GetExConfigTypeIconPath(ButtonConfig cfg)
        {
            if (!string.IsNullOrEmpty(cfg.icon))
                return GetPath(cfg.icon);

            if (ButtonConfigDefine.TypeIconMap.TryGetValue(cfg.configType, out string iconFileName))
                return GetPath(iconFileName, IconFolderPrefix);

            return GetPath("default.ico", IconFolderPrefix);
        }

        /// <summary>
        /// 按钮配置路径
        /// </summary>
        public static string GetButtonConfigPath() => GetPath("Asset/Resource/Config/config.json");
        
    }
}