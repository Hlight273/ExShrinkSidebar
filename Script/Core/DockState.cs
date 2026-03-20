using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Model;
using ExShrinkSidebar.Script.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace ExShrinkSidebar.Script.Core
{
    public static class DockState
    {
        private  static DockModel model=new DockModel
        {
            edge = DockEdge.Left,
            screenIndex = 0
        };
        public static DockEdge CurrentEdge { get=>model.edge; private set => model.edge = value; }
        public static int CurrentScreenIndex { get => model.screenIndex; private set => model.screenIndex = value; }

        public static DockOrientation curDockOrientation
        {
            get => (CurrentEdge == DockEdge.Left || CurrentEdge == DockEdge.Right) ? DockOrientation.Vertical : DockOrientation.Horizontal;
        }

        

        public static void SetEdge(DockEdge edge)
        {
            if (ScreenHelper.IsEdgeConnected(CurrentScreenIndex, edge)) //如果当前屏幕的边缘已经连接了其他窗口，则不能切换
                return;
            if (CurrentEdge != edge)
            {
                CurrentEdge = edge;
                EVENT.emit(EventIds.ON_DOCK_MODEL_CHANGED, new VoidEvent() { });
            }
        }

        public static void SetScreenIndex(int index)
        {
            if (CurrentScreenIndex != index)
            {
                CurrentScreenIndex = index;
                EVENT.emit(EventIds.ON_DOCK_MODEL_CHANGED, new VoidEvent() { });
            }
        }
    }

}
