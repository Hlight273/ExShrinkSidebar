namespace ExShrinkSidebar.Script.Model
{
    public class AppConfig
    {
        public List<ButtonConfig> buttons { get; set; }
    }

    public class ButtonConfig
    {
        public string name { get; set; }
        public string icon { get; set; }
        public string script { get; set; }
    }
}