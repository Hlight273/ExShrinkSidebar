using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Localization;
using ExShrinkSidebar.Script.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ExShrinkSidebar.UI.Views.EditConfigView
{
    public partial class AddConfigPanel : Window
    {
        private AppConfig _workingConfig = new AppConfig();
        private List<ButtonConfig> _rootList = new List<ButtonConfig>();

        public AddConfigPanel()
        {
            InitializeComponent();
            EVENT.on<ConfigNodeEventArg>(EventIds.ON_CONFIG_NODE_DELETE_REQUEST, OnNodeDeleteRequest);
            EVENT.on<ConfigNodeReorderEventArg>(EventIds.ON_CONFIG_NODE_REORDER_REQUEST, OnNodeReorderRequest);
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
                    Margin = new Thickness(0, 0, 0, 10),
                    Padding = new Thickness(10),
                    CornerRadius = new CornerRadius(8),
                    BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#334D5966")),
                    BorderThickness = new Thickness(1),
                    Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#14343C47"))
                };

                var content = new StackPanel();
                var header = new TextBlock
                {
                    Text = UiTextCatalog.Get(UiTextCatalog.RootCardTitle),
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                var view = new SubConfigView();
                view.SetData(cfg, 0, null);
                content.Children.Add(header);
                content.Children.Add(view);
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

        private void OnNodeReorderRequest(object sender, ConfigNodeReorderEventArg e)
        {
            if (e?.DragNode == null || e.TargetNode == null || ReferenceEquals(e.DragNode, e.TargetNode))
            {
                return;
            }

            var dragParent = e.DragNode.Parent;
            var targetParent = e.TargetNode.Parent;
            if (!ReferenceEquals(dragParent, targetParent))
            {
                return;
            }

            var sourceList = dragParent == null ? _rootList : dragParent.logicChain;
            if (sourceList == null)
            {
                return;
            }

            var dragIndex = sourceList.IndexOf(e.DragNode);
            var targetIndex = sourceList.IndexOf(e.TargetNode);
            if (dragIndex < 0 || targetIndex < 0)
            {
                return;
            }

            sourceList.RemoveAt(dragIndex);

            if (dragIndex < targetIndex)
            {
                targetIndex--;
            }

            var insertIndex = e.InsertAfter ? targetIndex + 1 : targetIndex;
            if (insertIndex < 0)
            {
                insertIndex = 0;
            }

            if (insertIndex > sourceList.Count)
            {
                insertIndex = sourceList.Count;
            }

            sourceList.Insert(insertIndex, e.DragNode);
            RenderTree();
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
            base.OnClosed(e);
            EVENT.off<ConfigNodeEventArg>(EventIds.ON_CONFIG_NODE_DELETE_REQUEST, OnNodeDeleteRequest);
            EVENT.off<ConfigNodeReorderEventArg>(EventIds.ON_CONFIG_NODE_REORDER_REQUEST, OnNodeReorderRequest);
            EVENT.off<LanguageChangedEventArg>(EventIds.ON_LANGUAGE_CHANGED, OnLanguageChanged);
        }
    }
}
