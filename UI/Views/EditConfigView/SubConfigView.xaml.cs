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
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using Point = System.Windows.Point;

namespace ExShrinkSidebar.UI.Views.EditConfigView
{
    public partial class SubConfigView : System.Windows.Controls.UserControl
    {
        private ButtonConfig _cfg;
        private int _depth;
        private bool _isBinding;
        private Point _dragStartPoint;
        private const int INDENT_WIDTH = 20;

        public SubConfigView()
        {
            InitializeComponent();
            TypeComboBox.ItemsSource = Enum.GetValues(typeof(ExConfigType));
            ConfigNameLabelText.Text = UiTextCatalog.Get(UiTextCatalog.ConfigNameLabel);
            FunctionLabelText.Text = UiTextCatalog.Get(UiTextCatalog.FunctionLabel);
        }

        public void SetData(ButtonConfig cfg, int depth = 0, ButtonConfig parent = null)
        {
            _isBinding = true;
            _cfg = cfg;
            _depth = depth;
            _cfg.Parent = parent;

            IndentPad.Width = depth * INDENT_WIDTH;
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

        private void ResetDropIndicator()
        {
            NodeBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444"));
            NodeBorder.BorderThickness = new Thickness(0, 0, 0, 1);
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
                view.SetData(child, _depth + 1, _cfg);
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

        private void NodeBorder_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(this);
        }

        private void NodeBorder_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed || _cfg == null)
            {
                return;
            }

            var currentPosition = e.GetPosition(this);
            if (Math.Abs(currentPosition.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPosition.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            DragDrop.DoDragDrop(this, new DataObject(typeof(ButtonConfig), _cfg), DragDropEffects.Move);
        }

        private void NodeBorder_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ButtonConfig)))
            {
                e.Effects = DragDropEffects.None;
                ResetDropIndicator();
                return;
            }

            var dragNode = e.Data.GetData(typeof(ButtonConfig)) as ButtonConfig;
            if (dragNode == null || ReferenceEquals(dragNode, _cfg) || !ReferenceEquals(dragNode.Parent, _cfg.Parent))
            {
                e.Effects = DragDropEffects.None;
                ResetDropIndicator();
                return;
            }

            e.Effects = DragDropEffects.Move;
            e.Handled = true;

            var position = e.GetPosition(NodeBorder);
            var insertAfter = position.Y > NodeBorder.ActualHeight / 2;
            NodeBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF688BC3"));
            NodeBorder.BorderThickness = insertAfter ? new Thickness(0, 0, 0, 2) : new Thickness(0, 2, 0, 1);
        }

        private void NodeBorder_DragLeave(object sender, DragEventArgs e)
        {
            ResetDropIndicator();
        }

        private void NodeBorder_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent(typeof(ButtonConfig)))
                {
                    return;
                }

                var dragNode = e.Data.GetData(typeof(ButtonConfig)) as ButtonConfig;
                if (dragNode == null || ReferenceEquals(dragNode, _cfg) || !ReferenceEquals(dragNode.Parent, _cfg.Parent))
                {
                    return;
                }

                var position = e.GetPosition(NodeBorder);
                var insertAfter = position.Y > NodeBorder.ActualHeight / 2;

                EVENT.emit(EventIds.ON_CONFIG_NODE_REORDER_REQUEST, this, new ConfigNodeReorderEventArg
                {
                    DragNode = dragNode,
                    TargetNode = _cfg,
                    InsertAfter = insertAfter
                });
            }
            finally
            {
                ResetDropIndicator();
            }
        }
    }
}
