using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Core.Event;

namespace ExShrinkSidebar.UI.Views.MainWindow
{
    public partial class DockPositionToggle : System.Windows.Controls.UserControl
    {
        public DockPositionToggle()
        {
            InitializeComponent();
        }

        private void onClickBtn(DockEdge edgeType, object sender, System.Windows.RoutedEventArgs e)
        {
            DockState.SetEdge(edgeType);
            //EVENT.emit(EventIds.ON_DOCK_EDGE_CHANGED, this, new DockEdgeArg() { Edge = edgeType });
        }

        private void OnClickTop(object sender, System.Windows.RoutedEventArgs e) => onClickBtn(DockEdge.Top, sender, e);
        private void OnClickBottom(object sender, System.Windows.RoutedEventArgs e) => onClickBtn(DockEdge.Bottom, sender, e);
        private void OnClickLeft(object sender, System.Windows.RoutedEventArgs e) => onClickBtn(DockEdge.Left, sender, e);
        private void OnClickRight(object sender, System.Windows.RoutedEventArgs e) => onClickBtn(DockEdge.Right, sender, e);
    }
}