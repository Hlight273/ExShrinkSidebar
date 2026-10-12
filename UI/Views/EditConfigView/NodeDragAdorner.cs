using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;

namespace ExShrinkSidebar.UI.Views.EditConfigView
{
    /// <summary>
    /// 绘制节点快照和目标外框；不参与命中、不改变配置树布局。
    /// </summary>
    internal sealed class NodeDragAdorner : Adorner
    {
        private readonly VisualCollection _visuals;
        private readonly DrawingVisual _preview = new DrawingVisual();
        private readonly DrawingVisual _indicator = new DrawingVisual();
        private readonly TranslateTransform _translation = new TranslateTransform();
        private readonly Point _grabOffset;
        private Rect _lastIndicator = Rect.Empty;
        private Rect _lastViewport = Rect.Empty;

        /// <summary>
        /// 按源节点 DPI 生成一次快照，裁掉树缩进空白并保存鼠标抓取偏移。
        /// </summary>
        internal NodeDragAdorner(UIElement host, FrameworkElement row, double indent, Point grabOffset)
            : base(host)
        {
            // 先发布空集合；属性变更和添加子节点都可能让 WPF 回调子节点查询。
            _visuals = new VisualCollection(this);
            IsHitTestVisible = false;
            _grabOffset = new Point(grabOffset.X - indent, grabOffset.Y);
            var size = new Size(Math.Max(1, row.ActualWidth - indent), Math.Max(1, row.ActualHeight));
            var dpi = VisualTreeHelper.GetDpi(row);
            var drawing = new DrawingVisual();
            using (var context = drawing.RenderOpen())
            {
                var brush = new VisualBrush(row)
                {
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = new Rect(indent, 0, size.Width, size.Height),
                    Stretch = Stretch.Fill
                };
                context.DrawRectangle(brush, null, new Rect(size));
            }

            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(size.Width * dpi.DpiScaleX),
                (int)Math.Ceiling(size.Height * dpi.DpiScaleY), dpi.PixelsPerInchX, dpi.PixelsPerInchY,
                PixelFormats.Pbgra32);
            bitmap.Render(drawing);
            bitmap.Freeze();

            // 将背景、阴影与快照一次性绘制，逐帧只更新位移，避免重建控件和重复截图。
            using (var context = _preview.RenderOpen())
            {
                context.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(65, 0, 0, 0)), null,
                    new Rect(3, 4, size.Width, size.Height), 6, 6);
                context.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(33, 39, 49)),
                    new Pen(new SolidColorBrush(Color.FromRgb(106, 158, 232)), 1), new Rect(size), 5, 5);
                context.DrawImage(bitmap, new Rect(size));
            }
            _preview.Opacity = 0.86;
            _preview.Transform = _translation;
            // 目标外框位于预览上方，避免浮动节点遮挡提示。
            _visuals.Add(_preview);
            _visuals.Add(_indicator);
        }

        /// <summary>
        /// 更新跟随位置与视口裁剪，仅在目标边界变化时重绘完整外框。
        /// </summary>
        internal void Update(Point pointer, Rect viewport, Rect targetBounds, bool visible)
        {
            _translation.X = pointer.X - _grabOffset.X;
            _translation.Y = pointer.Y - _grabOffset.Y;
            _preview.Opacity = visible ? 0.86 : 0;
            if (_lastViewport != viewport)
            {
                Clip = new RectangleGeometry(viewport);
                _lastViewport = viewport;
            }
            if (_lastIndicator == targetBounds)
            {
                return;
            }
            _lastIndicator = targetBounds;
            using var context = _indicator.RenderOpen();
            if (!targetBounds.IsEmpty && targetBounds.Width > 2 && targetBounds.Height > 2)
            {
                var brush = new SolidColorBrush(Color.FromRgb(130, 174, 240));
                // 描边向内收一个像素，不占布局空间，也不会横穿组合的自身行与子树。
                targetBounds.Inflate(-1, -1);
                context.DrawRoundedRectangle(null, new Pen(brush, 2), targetBounds, 8, 8);
            }
        }

        /// <summary>构造早期尚未发布集合时没有子节点，初始化后返回实际节点数。</summary>
        protected override int VisualChildrenCount => _visuals?.Count ?? 0;

        /// <summary>向 WPF 提供指定索引的绘制节点。</summary>
        protected override Visual GetVisualChild(int index)
        {
            // 与构造早期的零子节点计数保持一致，非法索引遵循 WPF 的访问契约。
            if (_visuals == null || index < 0 || index >= _visuals.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return _visuals[index];
        }
    }
}
