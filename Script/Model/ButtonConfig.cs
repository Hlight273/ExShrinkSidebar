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
        public string name { get; set; } = string.Empty;
        public string icon { get; set; } = string.Empty;

        public ExConfigType configType { get; set; }
        public List<ButtonConfig> logicChain { get; set; } = new List<ButtonConfig>();
        public ButtonConfigNodeArg arg { get; set; } = new ButtonConfigNodeArg();

        [JsonIgnore]
        public bool useChainLogic { get => configType == ExConfigType.Combine; }
        [JsonIgnore]
        public ButtonConfig Parent { get; set; }
        [JsonIgnore]
        public bool IsExpanded { get; set; } = true;
    }

    public class ButtonConfigNodeArg
    {
        public string path { get; set; } = string.Empty;
        public string script { get; set; } = string.Empty;
        public string arguments { get; set; } = string.Empty;
        public ExWindowCorner? windowCorner { get; set; }
        public int windowWidth { get; set; }
        public int windowHeight { get; set; }
    }

    public enum ExConfigType
    {
        [Description("ExConfigType_Combine")]
        Combine = 0,
        [Description("ExConfigType_OpenFolder")]
        OpenFolder,
        [Description("ExConfigType_Execute")]
        Execute,
        [Description("ExConfigType_Notepad")]
        Notepad,
    }

    public enum ExWindowCorner
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3
    }

    public static class ButtonConfigDefine
    {
        public static readonly Dictionary<ExConfigType, string> TypeIconMap = new Dictionary<ExConfigType, string>
        {
            { ExConfigType.OpenFolder, "folder.ico" },
            { ExConfigType.Execute, "terminal.ico" },
            { ExConfigType.Notepad, "notepad.ico" },
            { ExConfigType.Combine, "combine.ico" }
        };
    }
}
