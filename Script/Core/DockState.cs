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
        public  static DockModel _model=new DockModel
        {
            edge = DockEdge.Left,
            screenIndex = 0
        };
        public static DockModel Model
        {
            get => _model;
            private set => _model = value;
        }
        public static DockEdge CurrentEdge { get=>_model.edge; private set => _model.edge = value; }
        public static int CurrentScreenIndex { get => _model.screenIndex; private set => _model.screenIndex = value; }

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
                var oldModel = (DockModel)_model.Clone();
                CurrentEdge = edge;
                var newModel = (DockModel)_model.Clone();
                EVENT.emit(EventIds.ON_DOCK_MODEL_CHANGED, new DockModelArg(oldModel, newModel));
            }
        }

        public static void SetScreenIndex(int index)
        {
            if (!ScreenHelper.IsValidScreenIndex(index) || CurrentScreenIndex == index)
            {
                return;
            }

            var oldModel = (DockModel)_model.Clone();

            if (!TryGetAvailableEdge(index, CurrentEdge, out var edge))
            {
                return;
            }

            CurrentScreenIndex = index;
            CurrentEdge = edge;
            var newModel = (DockModel)_model.Clone();
            EVENT.emit(EventIds.ON_DOCK_MODEL_CHANGED, new DockModelArg(oldModel, newModel));
        }

        private static bool TryGetAvailableEdge(int screenIndex, DockEdge preferredEdge, out DockEdge edge)
        {
            if (!ScreenHelper.IsEdgeConnected(screenIndex, preferredEdge))
            {
                edge = preferredEdge;
                return true;
            }

            foreach (var candidate in Enum.GetValues<DockEdge>())
            {
                if (!ScreenHelper.IsEdgeConnected(screenIndex, candidate))
                {
                    edge = candidate;
                    return true;
                }
            }

            edge = preferredEdge;
            return false;
        }
    }

}
