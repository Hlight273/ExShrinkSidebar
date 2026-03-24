using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Model;
using ExShrinkSidebar.Script.Utils;
using ExShrinkSidebar.UI.ViewModels.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Forms = System.Windows.Forms;

namespace ExShrinkSidebar.UI.Views.MainWindow
{
    public partial class MainWindow : WindowBase
    {
        private DockManager? dock;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += Init;
        }

        protected override void RegisterEvents()
        {
            EVENT.on<DockModelArg>(EventIds.ON_DOCK_MODEL_CHANGED, (sender, e) => updateView(e.oldModel,e.newModel));
            EVENT.on<ConfigSavedEventArg>(EventIds.ON_CONFIG_SAVED, (sender, e) => refreshButtons());
        }

        void Init(object sender, RoutedEventArgs e)
        {
            dock = new DockManager(this);
            DockState.SetEdge(DockEdge.Top);
            updateView(null, DockState.Model);
        }

        protected override void Cleanup()
        {
            dock?.Cleanup();
            dock = null;
            base.Cleanup();
        }

        async void updateView(DockModel oldModel, DockModel newModel)
        {
            bool edgeChanged = oldModel?.edge != newModel?.edge;
            if (edgeChanged) {
                Debug.WriteLine("Edge changed");
                this.Opacity = 0;
                dock.updateEdgePosition();
            }
            SetupDragBar();
            refreshButtons();
            this.Opacity = 1;


        }

        void refreshButtons()
        {
            var config = ConfigManager.Instance.Config;
            if (config?.buttons == null) return;

            int currentCount = ButtonPanel.Children.Count;
            int targetCount = config.buttons.Count;
            for (int i = targetCount; i < currentCount; i++)
            {
                if (i < ButtonPanel.Children.Count)
                {
                    ButtonPanel.Children[i].Visibility = Visibility.Collapsed;
                }
            }

            for (int i = 0; i < targetCount; i++)
            {
                var b = config.buttons[i];
                if (i < currentCount)
                {
                    if (ButtonPanel.Children[i] is IconButton existingBtn)
                    {
                        existingBtn.Visibility = Visibility.Visible;
                        existingBtn.SetData(b);
                    }
                }
                else
                {
                    var btn = new IconButton();
                    btn.SetData(b);
                    btn.Margin = new Thickness(8);
                    ButtonPanel.Children.Add(btn);
                }
            }
            var orientation = ScreenHelper.getMainLayoutOrientation(DockState.CurrentEdge);
            ButtonPanel.Orientation = InnerPanel.Orientation = orientation;
            
        }

        #region Drag Logic
        private bool isDragging = false;
        private System.Windows.Point startMouseScreen;
        private double startLeft;
        private double startTop;

        private double GetWindowWidthForClamp()
        {
            var width = Width;
            if (double.IsNaN(width) || width <= 0)
            {
                width = ActualWidth;
            }

            return width;
        }

        private double GetWindowHeightForClamp()
        {
            var height = Height;
            if (double.IsNaN(height) || height <= 0)
            {
                height = ActualHeight;
            }

            return height;
        }

        private Matrix TransformFromDevice
        {
            get
            {
                var source = PresentationSource.FromVisual(this);
                return source?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
            }
        }

        private System.Windows.Point PixelsToDip(System.Drawing.Point point) => TransformFromDevice.Transform(new System.Windows.Point(point.X, point.Y));

        private Rect ScreenBoundsToDip(System.Drawing.Rectangle bounds)
        {
            var topLeft = PixelsToDip(bounds.Location);
            var bottomRight = PixelsToDip(new System.Drawing.Point(bounds.Right, bounds.Bottom));
            return new Rect(topLeft, bottomRight);
        }

        private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;

            // 暂停Dock逻辑
            dock?.Pause();

            var cursorPos = Forms.Control.MousePosition;
            startMouseScreen = PixelsToDip(cursorPos);

            startLeft = Left;
            startTop = Top;

            Mouse.Capture(DragBar, CaptureMode.Element);
        }

        private void DragBar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isDragging) return;

            var cursorPos = Forms.Control.MousePosition;
            var screenPos = PixelsToDip(cursorPos);

            double dx = screenPos.X - startMouseScreen.X;
            double dy = screenPos.Y - startMouseScreen.Y;

            var bound = ScreenBoundsToDip(ScreenHelper.GetBounds(DockState.CurrentScreenIndex));

            switch (DockState.CurrentEdge)
            {
                case DockEdge.Top:
                case DockEdge.Bottom:
                    var width = GetWindowWidthForClamp();

                    double newLeft = startLeft + dx;

                    if (newLeft < bound.Left)
                        newLeft = bound.Left;

                    if (newLeft + width > bound.Right)
                        newLeft = bound.Right - width;

                    Left = newLeft;

                    break;

                case DockEdge.Left:
                case DockEdge.Right:
                    var height = GetWindowHeightForClamp();

                    double newTop = startTop + dy;

                    if (newTop < bound.Top)
                        newTop = bound.Top;

                    if (newTop + height > bound.Bottom)
                        newTop = bound.Bottom - height;

                    Top = newTop;

                    break;
            }
        }

        private void DragBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;

            Mouse.Capture(null);

            dock?.Resume();
        }

        #endregion

        #region DragBar Layout
        private const int DRAG_BAR_SIZE = 10;
        private static readonly System.Windows.Input.Cursor HORIZONTAL_CURSOR = System.Windows.Input.Cursors.SizeWE;
        private static readonly System.Windows.Input.Cursor VERTICAL_CURSOR = System.Windows.Input.Cursors.SizeNS;

        void SetupDragBar()
        {
            if (DockState.curDockOrientation == DockOrientation.Horizontal)// 水平方向
            {
                DragBar.Height = DRAG_BAR_SIZE;
                DragBar.Width = double.NaN;
                DragBar.VerticalAlignment = DockState.CurrentEdge == DockEdge.Top
                    ? VerticalAlignment.Bottom
                    : VerticalAlignment.Top;
                DragBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                DragBar.Cursor = HORIZONTAL_CURSOR;
            }
            else// 垂直方向
            {
                DragBar.Width = DRAG_BAR_SIZE;
                DragBar.Height = double.NaN;
                DragBar.HorizontalAlignment = DockState.CurrentEdge == DockEdge.Left
                    ? System.Windows.HorizontalAlignment.Right
                    : System.Windows.HorizontalAlignment.Left;
                DragBar.VerticalAlignment = VerticalAlignment.Stretch;
                DragBar.Cursor = VERTICAL_CURSOR;
            }
        }
        #endregion
    }
}
