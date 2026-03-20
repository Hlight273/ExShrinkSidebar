using System;
using System.IO;
using System.Text.Json;
using System.Linq;
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
            configPath = ResHelper.GetButtonConfigPath();
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
                        id = 1,
                        name = "记事本",
                        configType = ExConfigType.Notepad
                    },
                }
            };
        }



        /// <summary>
        /// 更新内存中的配置并立即保存到磁盘
        /// </summary>
        /// <param name="newConfig">新的配置对象</param>
        public void UpdateAndSave(AppConfig newConfig)
        {
            Config = newConfig ?? new AppConfig();
            Save();
        }

        /// <summary>
        /// 更新按钮列表配置并保存（快捷方法）
        /// </summary>
        /// <param name="buttons">新的按钮列表</param>
        public void UpdateButtonsAndSave(List<ButtonConfig> buttons)
        {
            Config ??= new AppConfig();
            Config.buttons = buttons;
            Save();
        }

        /// <summary>
        /// 添加一个新的按钮配置（自动生成递增ID）并保存
        /// </summary>
        /// <param name="button">新按钮配置（无需设置ID）</param>
        public void AddButton(ButtonConfig button)
        {
            Config ??= new AppConfig();
            Config.buttons ??= new List<ButtonConfig>();
            long newId = Config.buttons.Any() ? Config.buttons.Max(b => b.id) + 1 : 1;
            button.id = newId;
            Config.buttons.Add(button);
            Save();
        }

        /// <summary>
        /// 根据ID删除指定的按钮配置并保存
        /// </summary>
        /// <param name="id">要删除的按钮ID</param>
        /// <returns>是否成功删除（找不到ID返回false）</returns>
        public bool RemoveButton(long id)
        {
            if (Config?.buttons == null) return false;
            var itemToRemove = Config.buttons.FirstOrDefault(b => b.id == id);
            if (itemToRemove != null)
            {
                Config.buttons.Remove(itemToRemove);
                Save();
                return true;
            }
            return false;
        }

        public long GetNextId()
        {
            if (Config?.buttons == null || !Config.buttons.Any()) return 1;

            // 递归查找所有节点中的最大 ID (包括子节点)
            long maxId = 0;
            void FindMax(List<ButtonConfig> nodes)
            {
                if (nodes == null) return;
                foreach (var node in nodes)
                {
                    if (node.id > maxId) maxId = node.id;
                    if (node.logicChain != null) FindMax(node.logicChain);
                }
            }

            FindMax(Config.buttons);
            return maxId + 1;
        }
    }
}