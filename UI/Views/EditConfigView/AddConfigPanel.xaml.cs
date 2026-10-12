using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Localization;
using ExShrinkSidebar.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Media;

namespace ExShrinkSidebar.UI.Views.EditConfigView
{
    public partial class AddConfigPanel : Window
    {
        private AppConfig _workingConfig = new AppConfig();
        private List<ButtonConfig> _rootList = new List<ButtonConfig>();
        private NodeDragAdorner? _dragAdorner;
        private ButtonConfig? _dragNode;
        private SubConfigView? _dropTarget;
        private NodeReorderSession? _reorderSession;
        // 自动滚动的边缘触发距离，单位为 WPF 设备无关像素。
        private const double DragScrollEdgeSize = 48;
        // 指针贴近边缘时的最大滚动速度，单位为设备无关像素/秒。
        private const double DragScrollMaxSpeed = 600;
        // 以约每秒 60 次的频率更新滚动，静止指针也能持续触发。
        private readonly DispatcherTimer _dragScrollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        public AddConfigPanel()
        {
            InitializeComponent();
            _dragScrollTimer.Tick += OnDragScrollTick;
            EVENT.on<ConfigNodeEventArg>(EventIds.ON_CONFIG_NODE_DELETE_REQUEST, OnNodeDeleteRequest);
            EVENT.on<LanguageChangedEventArg>(EventIds.ON_LANGUAGE_CHANGED, OnLanguageChanged);
        }

        public void Init()
        {
            TitleText.Text = UiTextCatalog.Get(UiTextCatalog.ConfigEditorTitle);
            AddRootBtn.ToolTip = UiTextCatalog.Get(UiTextCatalog.AddRootTooltip);
            AddRootBtn.Content = UiTextCatalog.Get(UiTextCatalog.AddRootTooltip);
            ConfirmBtn.Content = UiTextCatalog.Get(UiTextCatalog.SaveAndClose);
            _workingConfig = ConfigManager.Instance.CloneConfig();
            _rootList = _workingConfig.buttons;
            RenderTree();
        }

        private void CreateNewConfig()
        {
            var newCfg = new ButtonConfig
            {
                id = ConfigManager.Instance.ReserveNextId(),
                name = UiTextCatalog.Get(UiTextCatalog.NewConfigName),
                configType = ExConfigType.Execute
            };

            _rootList.Add(newCfg);
        }

        private void RenderTree()
        {
            TreeContainer.Children.Clear();
            if (_rootList == null)
            {
                return;
            }

            foreach (var cfg in _rootList)
            {
                var card = new Border
                {
                    Margin = new Thickness(0, 0, 0, 14),
                    Padding = new Thickness(12, 10, 12, 6),
                    CornerRadius = new CornerRadius(10),
                    BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#344052")),
                    BorderThickness = new Thickness(1),
                    Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#212731"))
                };

                var content = new StackPanel();
                var header = new TextBlock
                {
                    Text = UiTextCatalog.Get(UiTextCatalog.RootCardTitle),
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(135, 163, 201)),
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(4, 0, 0, 3)
                };
                var view = new SubConfigView();
                view.SetData(cfg, 0, null, TreeContainer.Children.Count + 1);
                // 让卡片标题和内边距也能定位到对应根节点。
                card.Tag = view;
                content.Children.Add(header);
                content.Children.Add(view);
                // 独立底部区域容纳根配置抓取角，不遮住展开子树或窗口缩放角。
                var rootHandle = new System.Windows.Controls.Control
                {
                    Style = (Style)FindResource("RootDragHandleStyle"),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    Margin = new Thickness(0, 4, 0, 0),
                    ToolTip = UiTextCatalog.Get(UiTextCatalog.DragToReorder)
                };
                rootHandle.PreviewMouseLeftButtonDown += view.DragHandle_MouseDown;
                rootHandle.PreviewMouseMove += view.DragHandle_MouseMove;
                rootHandle.PreviewMouseLeftButtonUp += view.DragHandle_EndCapture;
                rootHandle.LostMouseCapture += view.DragHandle_EndCapture;
                content.Children.Add(rootHandle);
                card.Child = content;
                TreeContainer.Children.Add(card);
            }
        }

        private void OnNodeDeleteRequest(object sender, ConfigNodeEventArg e)
        {
            var nodeToDelete = e.Data;
            if (nodeToDelete == null)
            {
                return;
            }

            if (nodeToDelete.Parent == null)
            {
                _rootList.Remove(nodeToDelete);
            }
            else
            {
                nodeToDelete.Parent.logicChain.Remove(nodeToDelete);
            }

            RenderTree();
        }

        private void OnAddRootClick(object sender, RoutedEventArgs e)
        {
            CreateNewConfig();
            RenderTree();
        }

        /// <summary>
        /// 在编辑器内运行节点拖拽，并确保松手、取消或异常时释放自动滚动状态。
        /// </summary>
        internal void DragNode(SubConfigView source, ButtonConfig node, System.Windows.Point grabOffset)
        {
            var layer = AdornerLayer.GetAdornerLayer(TreeScrollViewer);
            var sourceContainer = source.DragContainer;
            var previousOpacity = sourceContainer.Opacity;
            try
            {
                _dragNode = node;
                if (layer != null)
                {
                    _dragAdorner = new NodeDragAdorner(TreeScrollViewer, source.DragRow, source.DragIndent, grabOffset);
                    layer.Add(_dragAdorner);
                }
                // 仅隐藏视觉内容，保留占位，避免拖动开始时其他节点突然跳位。
                _reorderSession = new NodeReorderSession(source);
                sourceContainer.Opacity = 0;
                UpdateDragFeedback();
                source.GiveFeedback += OnNodeGiveFeedback;
                CompositionTarget.Rendering += OnDragRendering;
                _dragScrollTimer.Start();
                System.Windows.DragDrop.DoDragDrop(source,
                    new System.Windows.DataObject(typeof(ButtonConfig), node),
                    System.Windows.DragDropEffects.Move);
            }
            finally
            {
                _dragScrollTimer.Stop();
                _reorderSession?.Dispose();
                _reorderSession = null;
                CompositionTarget.Rendering -= OnDragRendering;
                source.GiveFeedback -= OnNodeGiveFeedback;
                if (_dragAdorner != null)
                {
                    layer?.Remove(_dragAdorner);
                }
                _dragAdorner = null;
                _dragNode = null;
                _dropTarget = null;
                sourceContainer.Opacity = previousOpacity;
            }
        }

        /// <summary>以普通指针配合节点预览，保留无效落点的禁止光标。</summary>
        private void OnNodeGiveFeedback(object sender, System.Windows.GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            System.Windows.Input.Mouse.SetCursor(e.Effects == System.Windows.DragDropEffects.Move
                ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.No);
            e.Handled = true;
        }

        /// <summary>随渲染帧更新预览，并重新命中滚动后的节点。</summary>
        private void OnDragRendering(object? sender, EventArgs e)
        {
            UpdateDragFeedback();
        }

        /// <summary>
        /// 统一计算预览、插入提示与松手落点，防止滚动后提示和提交目标不一致。
        /// </summary>
        private void UpdateDragFeedback()
        {
            _dropTarget = null;
            if (_dragNode == null || !IsVisible ||
                TreeScrollViewer.Template.FindName("PART_ScrollContentPresenter", TreeScrollViewer)
                    is not ScrollContentPresenter viewport)
            {
                return;
            }

            var cursor = System.Windows.Forms.Cursor.Position;
            var pointer = TreeScrollViewer.PointFromScreen(new System.Windows.Point(cursor.X, cursor.Y));
            var viewportOrigin = viewport.TranslatePoint(new System.Windows.Point(), TreeScrollViewer);
            var viewportBounds = new Rect(viewportOrigin, viewport.RenderSize);
            var visible = viewportBounds.Contains(pointer);
            // 即时计算目标描边；拖拽期间不计时、不改变其他节点位置。
            var targetBounds = _reorderSession?.Update(TreeScrollViewer, pointer, visible) ?? Rect.Empty;
            _dropTarget = _reorderSession?.Target;
            _dragAdorner?.Update(pointer, viewportBounds, targetBounds, visible);
        }

        /// <summary>仅接受本编辑器正在拖动的节点，并以统一命中结果反馈可放置状态。</summary>
        private void TreeScrollViewer_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            UpdateDragFeedback();
            e.Effects = _dropTarget != null && _dragNode != null &&
                ReferenceEquals(e.Data.GetData(typeof(ButtonConfig)), _dragNode)
                ? System.Windows.DragDropEffects.Move : System.Windows.DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>松手时重新核对落点，只提交一次同级排序。</summary>
        private void TreeScrollViewer_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {
            UpdateDragFeedback();
            e.Handled = true;
            e.Effects = System.Windows.DragDropEffects.None;
            if (_dropTarget == null || _dragNode == null ||
                !ReferenceEquals(e.Data.GetData(typeof(ButtonConfig)), _dragNode))
            {
                return;
            }
            e.Effects = System.Windows.DragDropEffects.Move;
            _reorderSession?.Commit(_dragNode.Parent == null ? _rootList : _dragNode.Parent.logicChain);
        }

        /// <summary>
        /// 根据当前屏幕指针位置在内容视口边缘持续滚动，不依赖鼠标移动事件。
        /// </summary>
        private void OnDragScrollTick(object? sender, EventArgs e)
        {
            if (!IsVisible || (System.Windows.Forms.Control.MouseButtons & System.Windows.Forms.MouseButtons.Left) == 0)
            {
                _dragScrollTimer.Stop();
                return;
            }

            // 使用内容视口排除滚动条、标题栏和底部按钮；屏幕坐标转换同时处理 DPI。
            if (TreeScrollViewer.Template.FindName("PART_ScrollContentPresenter", TreeScrollViewer)
                is not ScrollContentPresenter viewport || viewport.ActualHeight <= 0)
            {
                return;
            }

            // OLE 拖拽循环中 WPF 鼠标位置可能不更新，因此读取系统指针位置。
            var cursor = System.Windows.Forms.Cursor.Position;
            var position = viewport.PointFromScreen(new System.Windows.Point(cursor.X, cursor.Y));
            if (position.X < 0 || position.X >= viewport.ActualWidth ||
                position.Y < 0 || position.Y >= viewport.ActualHeight)
            {
                return;
            }

            var edgeSize = Math.Min(DragScrollEdgeSize, viewport.ActualHeight / 2);
            double directionAndSpeed = 0;
            if (position.Y < edgeSize)
            {
                directionAndSpeed = -(1 - position.Y / edgeSize);
            }
            else if (position.Y > viewport.ActualHeight - edgeSize)
            {
                directionAndSpeed = 1 - (viewport.ActualHeight - position.Y) / edgeSize;
            }

            if (directionAndSpeed != 0)
            {
                // 限制到有效偏移范围，避免抵达首尾后继续请求越界滚动。
                var offset = TreeScrollViewer.VerticalOffset +
                    directionAndSpeed * DragScrollMaxSpeed * _dragScrollTimer.Interval.TotalSeconds;
                TreeScrollViewer.ScrollToVerticalOffset(Math.Clamp(offset, 0, TreeScrollViewer.ScrollableHeight));
            }
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            ConfigManager.Instance.UpdateAndSave(_workingConfig);
            EVENT.emit(EventIds.ON_CONFIG_SAVED, this, new ConfigSavedEventArg());
            Close();
        }

        private void OnDragMove(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResizeTopLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ResizeFromTop(e.VerticalChange);
            ResizeFromLeft(e.HorizontalChange);
        }

        private void ResizeTopRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ResizeFromTop(e.VerticalChange);
            ResizeFromRight(e.HorizontalChange);
        }

        private void ResizeBottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ResizeFromBottom(e.VerticalChange);
            ResizeFromLeft(e.HorizontalChange);
        }

        private void ResizeBottomRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ResizeFromBottom(e.VerticalChange);
            ResizeFromRight(e.HorizontalChange);
        }

        private void ResizeFromLeft(double horizontalChange)
        {
            var updatedWidth = Math.Max(MinWidth, Width - horizontalChange);
            var delta = Width - updatedWidth;
            Width = updatedWidth;
            Left += delta;
        }

        private void ResizeFromRight(double horizontalChange)
        {
            Width = Math.Max(MinWidth, Width + horizontalChange);
        }

        private void ResizeFromTop(double verticalChange)
        {
            var updatedHeight = Math.Max(MinHeight, Height - verticalChange);
            var delta = Height - updatedHeight;
            Height = updatedHeight;
            Top += delta;
        }

        private void ResizeFromBottom(double verticalChange)
        {
            Height = Math.Max(MinHeight, Height + verticalChange);
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArg e)
        {
            TitleText.Text = UiTextCatalog.Get(UiTextCatalog.ConfigEditorTitle);
            AddRootBtn.ToolTip = UiTextCatalog.Get(UiTextCatalog.AddRootTooltip);
            AddRootBtn.Content = UiTextCatalog.Get(UiTextCatalog.AddRootTooltip);
            ConfirmBtn.Content = UiTextCatalog.Get(UiTextCatalog.SaveAndClose);
            RenderTree();
        }

        protected override void OnClosed(EventArgs e)
        {
            _dragScrollTimer.Stop();
            _dragScrollTimer.Tick -= OnDragScrollTick;
            _reorderSession?.Dispose();
            _reorderSession = null;
            CompositionTarget.Rendering -= OnDragRendering;
            if (_dragAdorner != null)
            {
                AdornerLayer.GetAdornerLayer(TreeScrollViewer)?.Remove(_dragAdorner);
                _dragAdorner = null;
            }
            base.OnClosed(e);
            EVENT.off<ConfigNodeEventArg>(EventIds.ON_CONFIG_NODE_DELETE_REQUEST, OnNodeDeleteRequest);
            EVENT.off<LanguageChangedEventArg>(EventIds.ON_LANGUAGE_CHANGED, OnLanguageChanged);
        }
    }
}
