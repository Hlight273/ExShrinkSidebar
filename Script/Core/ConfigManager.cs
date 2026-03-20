using System;
using System.IO;
using System.Text.Json;
using ExShrinkSidebar.Script.Model;
using ExShrinkSidebar.Script.Utils;

namespace ExShrinkSidebar.Script.Core
{
    public sealed class ConfigManager
    {
        private static readonly Lazy<ConfigManager> _instance =
            new Lazy<ConfigManager>(() => new ConfigManager());

        public static ConfigManager Instance => _instance.Value;

        private readonly string configPath;

        public AppConfig Config { get; private set; }

        private ConfigManager()
        {
            configPath = ResHelper.GetPath("Asset/Resource/Config/config.json");

            Load();
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        public void Load()
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    CreateDefault();
                    Save();
                }
                else
                {
                    var json = File.ReadAllText(configPath);

                    Config = JsonSerializer.Deserialize<AppConfig>(json)
                             ?? new AppConfig();
                }
            }
            catch
            {
                Config = new AppConfig();
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void Save()
        {
            var dir = Path.GetDirectoryName(configPath);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(
                Config,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(configPath, json);
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        void CreateDefault()
        {
            Config = new AppConfig
            {
                buttons =
                {
                    new ButtonConfig
                    {
                        name = "记事本",
                        script = "notepad.exe"
                    },
                    new ButtonConfig
                    {
                        name = "CMD",
                        script = "cmd.exe"
                    }
                }
            };
        }
    }
}