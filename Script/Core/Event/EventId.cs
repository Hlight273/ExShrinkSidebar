using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExShrinkSidebar.Script.Core.Event
{
    public static class EventIds
    {
        //DOCK数据变更
        public const string ON_DOCK_MODEL_CHANGED = "ON_DOCK_EDGE_CHANGED";

        // config编辑器界面
        public const string ON_CONFIG_NODE_UPDATED = "ON_CONFIG_NODE_UPDATED";
        public const string ON_CONFIG_NODE_DELETE_REQUEST = "ON_CONFIG_NODE_DELETE_REQUEST";
        public const string ON_CONFIG_NODE_REORDER_REQUEST = "ON_CONFIG_NODE_REORDER_REQUEST";
        public const string ON_CONFIG_SAVED = "ON_CONFIG_SAVED";

        public const string ON_LANGUAGE_CHANGED = "ON_LANGUAGE_CHANGED";
    }
}
