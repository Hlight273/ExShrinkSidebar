using ExShrinkSidebar.Asset.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExShrinkSidebar.Script.Localization
{
    public static class LocalizationHelper
    {
        public static string GetLocalizedString(Enum value)
        {
            // 1. 获取枚举上的 Description 特性
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();

            if (attribute != null)
            {
                // 2. 根据 Key 从资源管理器中读取字符串
                // ResourceManager 会自动根据当前线程语言选择 .resx 文件
                var resourceManager = StringResources.ResourceManager;
                return resourceManager.GetString(attribute.Description) ?? value.ToString();
            }
            return value.ToString();
        }
    }
}
