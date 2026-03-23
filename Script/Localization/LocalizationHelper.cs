using ExShrinkSidebar.Asset.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace ExShrinkSidebar.Script.Localization
{
    public static class LocalizationHelper
    {
        public static readonly IReadOnlyList<string> SupportedCultures = new List<string> { "zh-CN", "en-US" };

        public static string GetLocalizedString(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field?.GetCustomAttribute<DescriptionAttribute>();

            if (attribute != null)
            {
                var resourceManager = StringResources.ResourceManager;
                return resourceManager.GetString(attribute.Description) ?? value.ToString();
            }

            return value.ToString();
        }

        public static void ApplyCulture(string cultureName)
        {
            var culture = new CultureInfo(cultureName);
            StringResources.Culture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
