namespace ExShrinkSidebar.Script.Model
{
    /// <summary>
    /// UI数据模型
    /// </summary>
    public class TestModel
    {
        public int Id { get; set; }
        public string Avatar { get; set; }
    }

    /// <summary>
    /// dock数据
    /// </summary>
    public class DockModel
    {
        public DockEdge edge { get; set; }
        public int screenIndex { get; set; }
    }
}
