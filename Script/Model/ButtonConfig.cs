using ExShrinkSidebar.Asset.Properties;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ExShrinkSidebar.Script.Model
{
    public class AppConfig
    {
        public List<ButtonConfig> buttons { get; set; } = new List<ButtonConfig>();
    }

    public class ButtonConfig
    {
        public long id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }

        public ExConfigType configType { get; set; }
        public List<ButtonConfig> logicChain { get; set; }
        public ButtonConfigNodeArg arg { get; set; }

        [JsonIgnore]
        public bool useChainLogic { get=>configType == ExConfigType.Combine; }
        [JsonIgnore]
        public ButtonConfig Parent { get; set; } //用于 UI 计算缩进深度的父节点引用

    }
    public class ButtonConfigNodeArg
    {
        public string path { get; set; }
        public string script { get; set; }
    }

    public enum ExConfigType
    {
        [Description("ExConfigType_Combine")]//本地化字段key
        Combine = 0,
        [Description("ExConfigType_OpenFolder")]
        OpenFolder,
        [Description("ExConfigType_Cmd")]
        Cmd,
        [Description("ExConfigType_Notepad")]
        Notepad,
    }

    public static class ButtonConfigDefine {
        public static readonly Dictionary<ExConfigType, string> TypeIconMap = new Dictionary<ExConfigType, string>
        {
            { ExConfigType.OpenFolder, "folder.ico" },
            { ExConfigType.Cmd, "terminal.ico" },
            { ExConfigType.Notepad, "notepad.ico" },
        };
    }
}