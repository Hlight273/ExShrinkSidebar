public struct MouseEdgeDistance
{
    public int Top;
    public int Bottom;
    public int Left;
    public int Right;

    public override string ToString()
    {
        return $"Top:{Top} Bottom:{Bottom} Left:{Left} Right:{Right}";
    }
}

public enum DockEdge
{
    Top,
    Bottom,
    Left,
    Right
}

public enum DockOrientation
{
    Horizontal = 0,
    Vertical = 1
}

namespace ExShrinkSidebar.Script.Utils
{
    public static class ScreenHelper
    {
        public static MouseEdgeDistance GetMouseEdgeDistance()
        {
            var pos = Control.MousePosition;

            var screen = Screen.FromPoint(pos);

            int left = pos.X - screen.Bounds.Left;
            int top = pos.Y - screen.Bounds.Top;

            return new MouseEdgeDistance
            {
                Top = top,
                Bottom = screen.Bounds.Height - top,
                Left = left,
                Right = screen.Bounds.Width - left
            };
        }

        public static Screen GetScreen(int index)
        {
            return Screen.AllScreens[index];
        }
        public static Rectangle GetBounds(int index)
        {
            return Screen.AllScreens[index].Bounds;
        }

        /// <summary>
        /// 判断点是否在屏幕内 xy为鼠标坐标
        /// <param name="screenIndex">多屏幕的索引</param>
        /// <param name="padding">屏幕边界扩展的像素,在该范围内，鼠标点可以认为在屏幕内</param>
        /// </summary>
        public static bool IsPointOnScreen(int x, int y, int screenIndex=0, int padding = 10)
        {
            var screens = Screen.AllScreens;
            if (screenIndex >= screens.Length)
                return false;

            var screen = screens[screenIndex];
            var bounds = screen.Bounds;

            var extendedBounds = new Rectangle(
                bounds.X - padding,
                bounds.Y - padding,
                bounds.Width + padding * 2,
                bounds.Height + padding * 2
            );
            return extendedBounds.Contains(x, y);
        }

        /// <summary>
        /// 获取主布局方向
        /// </summary>
        public static System.Windows.Controls.Orientation getMainLayoutOrientation(DockEdge edgeType)
        {
            if(edgeType==DockEdge.Left||edgeType==DockEdge.Right)
                return System.Windows.Controls.Orientation.Vertical;
            return System.Windows.Controls.Orientation.Horizontal;
        }

        /// <summary>
        /// 判断屏幕边缘是否连接到其他屏幕
        /// </summary>
        /// <param name="screenIndex">屏幕索引</param>
        /// <param name="edge">指定的屏幕边缘</param>
        public static bool IsEdgeConnected(int screenIndex, DockEdge edge)
        {
            if (screenIndex < 0 || screenIndex >= Screen.AllScreens.Length)
                return false;

            var currentScreen = Screen.AllScreens[screenIndex];
            var currentBounds = currentScreen.Bounds;

            foreach (var screen in Screen.AllScreens)
            {
                if (screen == currentScreen)
                    continue;

                var otherBounds = screen.Bounds;

                switch (edge)
                {
                    case DockEdge.Top:
                        if (Math.Abs(otherBounds.Bottom - currentBounds.Top) < 1 && // 垂直距离接近0
                            otherBounds.Right > currentBounds.Left && // 右边界超过左边界
                            otherBounds.Left < currentBounds.Right) // 左边界小于右边界
                            return true;
                        break;

                    case DockEdge.Bottom:
                        if (Math.Abs(otherBounds.Top - currentBounds.Bottom) < 1 &&
                            otherBounds.Right > currentBounds.Left &&
                            otherBounds.Left < currentBounds.Right)
                            return true;
                        break;

                    case DockEdge.Left:
                        if (Math.Abs(otherBounds.Right - currentBounds.Left) < 1 &&
                            otherBounds.Bottom > currentBounds.Top &&
                            otherBounds.Top < currentBounds.Bottom)
                            return true;
                        break;

                    case DockEdge.Right:
                        if (Math.Abs(otherBounds.Left - currentBounds.Right) < 1 &&
                            otherBounds.Bottom > currentBounds.Top &&
                            otherBounds.Top < currentBounds.Bottom)
                            return true;
                        break;
                }
            }

            return false;
        }


    }
}
