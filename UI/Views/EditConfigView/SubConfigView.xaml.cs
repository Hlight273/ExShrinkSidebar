using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Localization;
using ExShrinkSidebar.Script.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace ExShrinkSidebar.UI.Views.EditConfigView
{
    public partial class SubConfigView : System.Windows.Controls.UserControl
    {
        private ButtonConfig _cfg;
        private int _depth;
        private bool _isBinding;
        private Point _dragStartPoint;
        private Point _dragGrabOffset;
        private bool _canStartDrag;
        // 每一层配置树在节点行内增加的缩进，单位为设备无关像素。
        private const int INDENT_WIDTH = 20;

        /// <summary>提供当前节点数据，供编辑器校验同级落点。</summary>
        internal ButtonConfig ConfigNode => _cfg;

        /// <summary>提供自身节点行，不包含展开的子树。</summary>
        internal FrameworkElement DragRow => NodeBorder;

        /// <summary>根节点使用完整外层卡片，子节点使用包含展开子树的视图作为提示边界。</summary>
        internal FrameworkElement DragContainer => _cfg.Parent == null &&
            Parent is StackPanel content && content.Parent is Border card ? card : this;

        /// <summary>节点的树层级缩进，用于对齐快照及插入提示。</summary>
        internal double DragIndent => _depth * INDENT_WIDTH;

        /// <summary>名称后的只读同级序号；从 1 开始，不写入配置名称或旧序号字段。</summary>
        internal int SiblingOrdinal
        {
            set => OrdinalText.Text = UiTextCatalog.Format(UiTextCatalog.NodeOrdinal, value);
        }

        public SubConfigView()
        {
            InitializeComponent();
            TypeComboBox.ItemsSource = Enum.GetValues(typeof(ExConfigType));
            ConfigNameLabelText.Text = UiTextCatalog.Get(UiTextCatalog.ConfigNameLabel);
            FunctionLabelText.Text = UiTextCatalog.Get(UiTextCatalog.FunctionLabel);
        }

        /// <summary>绑定工作副本节点、树层级、父节点和从 1 开始的同级显示序号。</summary>
        public void SetData(ButtonConfig cfg, int depth = 0, ButtonConfig parent = null, int siblingOrdinal = 1)
        {
            _isBinding = true;
            _cfg = cfg;
            _depth = depth;
            _cfg.Parent = parent;
            SiblingOrdinal = siblingOrdinal;

            IndentPad.Width = depth * INDENT_WIDTH;
            ChildDragHandle.Visibility = parent == null ? Visibility.Collapsed : Visibility.Visible;
            ChildDragHandle.ToolTip = UiTextCatalog.Get(UiTextCatalog.DragToReorder);
            TypeComboBox.SelectedItem = cfg.configType;
            NameTextBox.Text = cfg.name ?? string.Empty;

            RefreshNodeDisplay();
            RenderChildren();
            _isBinding = false;
        }

        private void RefreshNodeDisplay()
        {
            SummaryText.Text = BuildSummary();
            UpdateNodeState();
        }

        private string BuildSummary()
        {
            _cfg.arg ??= new ButtonConfigNodeArg();

            switch (_cfg.configType)
            {
                case ExConfigType.Combine:
                    return UiTextCatalog.Format(UiTextCatalog.SummarySteps, _cfg.logicChain?.Count ?? 0);
                case ExConfigType.OpenFolder:
                    if (string.IsNullOrWhiteSpace(_cfg.arg.path))
                    {
                        return UiTextCatalog.Get(UiTextCatalog.SummaryNotConfiguredFolder);
                    }

                    var folderName = Path.GetFileName(_cfg.arg.path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    folderName = string.IsNullOrWhiteSpace(folderName) ? _cfg.arg.path : folderName;
                    return _cfg.arg.windowCorner == null
                        ? UiTextCatalog.Format(UiTextCatalog.SummaryOpenFolder, folderName)
                        : UiTextCatalog.Format(UiTextCatalog.SummaryOpenFolderWithCorner, folderName, _cfg.arg.windowCorner);
                case ExConfigType.Execute:
                    if (string.IsNullOrWhiteSpace(_cfg.arg.path))
                    {
                        return UiTextCatalog.Get(UiTextCatalog.SummaryNotConfiguredExecute);
                    }

                    var targetName = Path.GetFileName(_cfg.arg.path);
                    targetName = string.IsNullOrWhiteSpace(targetName) ? _cfg.arg.path : targetName;
                    return string.IsNullOrWhiteSpace(_cfg.arg.arguments)
                        ? UiTextCatalog.Format(UiTextCatalog.SummaryExecute, targetName)
                        : UiTextCatalog.Format(UiTextCatalog.SummaryExecute, $"{targetName} {_cfg.arg.arguments}");
                case ExConfigType.Notepad:
                    return UiTextCatalog.Get(UiTextCatalog.SummaryNotepad);
            }

            return string.Empty;
        }

        private void UpdateNodeState()
        {
            var isCombine = _cfg.useChainLogic;
            AddChildBtn.Visibility = isCombine ? Visibility.Visible : Visibility.Collapsed;
            ExpandBtn.Visibility = isCombine ? Visibility.Visible : Visibility.Collapsed;
            ChildrenContainer.Visibility = isCombine && _cfg.IsExpanded ? Visibility.Visible : Visibility.Collapsed;
            ExpandIconMask.OpacityMask = new ImageBrush(new BitmapImage(new Uri(_cfg.IsExpanded
                ? "pack://application:,,,/Asset/Resource/Icons/pandown.png"
                : "pack://application:,,,/Asset/Resource/Icons/panend.png")));
            MoreParamsBtn.Visibility = NeedsDetailPanel(_cfg.configType) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool NeedsDetailPanel(ExConfigType configType)
        {
            return configType == ExConfigType.OpenFolder || configType == ExConfigType.Execute;
        }

        private void RenderChildren()
        {
            ChildrenContainer.Children.Clear();
            if (!_cfg.useChainLogic || _cfg.logicChain == null)
            {
                return;
            }

            foreach (var child in _cfg.logicChain)
            {
                var view = new SubConfigView();
                view.SetData(child, _depth + 1, _cfg, ChildrenContainer.Children.Count + 1);
                ChildrenContainer.Children.Add(view);
            }
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_cfg == null)
            {
                return;
            }

            _cfg.name = NameTextBox.Text;
            EmitUpdate();
        }

        private void MoreParamsBtn_Click(object sender, RoutedEventArgs e)
        {
            var detailWindow = new NodeDetailWindow
            {
                Owner = Window.GetWindow(this)
            };
            detailWindow.Init(_cfg);

            if (detailWindow.ShowDialog() == true)
            {
                RefreshNodeDisplay();
                EmitUpdate();
            }
        }

        private void AddChildBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_cfg.useChainLogic)
            {
                return;
            }

            _cfg.logicChain ??= new List<ButtonConfig>();

            var newChild = new ButtonConfig
            {
                id = ConfigManager.Instance.ReserveNextId(),
                name = UiTextCatalog.Get(UiTextCatalog.NewStepName),
                configType = ExConfigType.Execute,
                Parent = _cfg
            };

            _cfg.logicChain.Add(newChild);
            _cfg.IsExpanded = true;

            RefreshNodeDisplay();
            RenderChildren();
            EmitUpdate();
        }

        private void ExpandBtn_Click(object sender, RoutedEventArgs e)
        {
            _cfg.IsExpanded = !_cfg.IsExpanded;
            UpdateNodeState();

            if (_cfg.IsExpanded)
            {
                RenderChildren();
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isBinding || TypeComboBox.SelectedItem == null || _cfg == null)
            {
                return;
            }

            if (TypeComboBox.SelectedItem is ExConfigType newType)
            {
                _cfg.configType = newType;

                if (_cfg.useChainLogic)
                {
                    _cfg.logicChain ??= new List<ButtonConfig>();
                }

                _cfg.arg ??= new ButtonConfigNodeArg();
                _cfg.IsExpanded = _cfg.useChainLogic;
                RefreshNodeDisplay();
                RenderChildren();
                EmitUpdate();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            EVENT.emit(EventIds.ON_CONFIG_NODE_DELETE_REQUEST, this, new ConfigNodeEventArg
            {
                Data = _cfg,
                ParentId = _cfg.Parent?.id ?? 0
            });
        }

        private void EmitUpdate()
        {
            EVENT.emit(EventIds.ON_CONFIG_NODE_UPDATED, this, new ConfigNodeEventArg
            {
                Data = _cfg
            });
        }

        /// <summary>
        /// 根节点抓取角和子节点握把共用入口，捕获鼠标以支持快速移出手柄后继续拖动。
        /// </summary>
        internal void DragHandle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement handle || _cfg == null)
            {
                return;
            }
            _dragStartPoint = e.GetPosition(handle);
            // 根手柄位于整卡底部，紧凑行预览改为从左下角抓取，避免出现整棵子树高度的偏移。
            _dragGrabOffset = _cfg.Parent == null
                ? new Point(Math.Clamp(_dragStartPoint.X, 0, NodeBorder.ActualWidth), Math.Max(0, NodeBorder.ActualHeight - 6))
                : e.GetPosition(NodeBorder);
            _canStartDrag = handle.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// 两类手柄超过系统阈值后释放捕获，将预览、滚动和排序交给编辑器。
        /// </summary>
        internal void DragHandle_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is not FrameworkElement handle)
            {
                return;
            }
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                _canStartDrag = false;
                if (handle.IsMouseCaptured)
                {
                    handle.ReleaseMouseCapture();
                }
                return;
            }
            if (!_canStartDrag || _cfg == null)
            {
                return;
            }
            var currentPosition = e.GetPosition(handle);
            if (Math.Abs(currentPosition.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPosition.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }
            _canStartDrag = false;
            handle.ReleaseMouseCapture();
            e.Handled = true;
            if (Window.GetWindow(this) is AddConfigPanel editor)
            {
                editor.DragNode(this, _cfg, _dragGrabOffset);
            }
        }

        /// <summary>点击后松手或捕获丢失时取消待启动拖拽，防止下一次移动误触发。</summary>
        internal void DragHandle_EndCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _canStartDrag = false;
            if (sender is FrameworkElement handle && handle.IsMouseCaptured)
            {
                handle.ReleaseMouseCapture();
            }
        }
    }
}
