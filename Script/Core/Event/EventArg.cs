using ExShrinkSidebar.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExShrinkSidebar.Script.Core.Event
{


    /// <summary>
    /// 无参数事件
    /// </summary>
    public class VoidEvent : EventBase{ }

    /// <summary>
    /// Dock数据更新
    /// </summary>
    public class DockModelArg : EventBase { 
        public DockModel oldModel { get; set; }
        public DockModel newModel { get; set; }
        public DockModelArg(DockModel oldModel, DockModel newModel)
        {
            this.oldModel = oldModel;
            this.newModel = newModel;
        }
    }

    /// <summary>
    ///测试
    /// </summary>
    public class TestArg
    {
        public bool IsCollapsed { get; set; }
        public double Width { get; set; }
        public string Mode { get; set; }  // "normal", "mini", "hidden"
    }
}
