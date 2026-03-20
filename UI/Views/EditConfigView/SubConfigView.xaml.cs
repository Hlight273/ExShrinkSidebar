using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Model;
using Microsoft.Win32; // 引用 OpenFileDialog 需要
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ExShrinkSidebar.UI.Views.EditConfigView
{
    public partial class SubConfigView : System.Windows.Controls.UserControl
    {
        private ButtonConfig _cfg;
        private int _depth = 0;
        private const int INDENT_WIDTH = 20;

        public SubConfigView()
        {
            InitializeComponent();
            InitTypeComboBox();
        }

        private void InitTypeComboBox()
        {
            TypeComboBox.ItemsSource = System.Enum.GetValues(typeof(ExConfigType));
        }

        public void SetData(ButtonConfig cfg, int depth = 0, ButtonConfig parent = null)
        {
            _cfg = cfg;
            _depth = depth;
            _cfg.Parent = parent;

            // 1. 处理缩进
            IndentPad.Width = depth * INDENT_WIDTH;

            // 2. 绑定数据到 UI
            TypeComboBox.SelectedItem = cfg.configType;

            // 绑定名称
            NameTextBox.Text = cfg.name ?? "";

            UpdateEditArea();

            // 3. 处理子节点递归
            if (cfg.useChainLogic)
            {
                ExpandBtn.Visibility = Visibility.Visible;
                // 如果有子节点，默认展开以便编辑
                if (_cfg.logicChain != null && _cfg.logicChain.Count > 0)
                {
                    ChildrenContainer.Visibility = Visibility.Visible;
                    ExpandBtn.Content = "-";
                }
                RenderChildren();
            }
            else
            {
                ExpandBtn.Visibility = Visibility.Collapsed;
                ChildrenContainer.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateEditArea()
        {
            ScriptBox.Visibility = Visibility.Collapsed;
            PathPanel.Visibility = Visibility.Collapsed;

            if (_cfg.arg == null) _cfg.arg = new ButtonConfigNodeArg();

            switch (_cfg.configType)
            {
                case ExConfigType.Cmd:
                case ExConfigType.Notepad:
                    ScriptBox.Visibility = Visibility.Visible;
                    ScriptBox.Text = _cfg.arg.script ?? "";
                    break;
                case ExConfigType.OpenFolder:
                    PathPanel.Visibility = Visibility.Visible;
                    PathBox.Text = _cfg.arg.path ?? "";
                    break;
            }
        }

        private void RenderChildren()
        {
            ChildrenContainer.Children.Clear();
            if (_cfg.logicChain == null) return;

            foreach (var child in _cfg.logicChain)
            {
                var view = new SubConfigView();
                view.SetData(child, _depth + 1, _cfg);
                ChildrenContainer.Children.Add(view);
            }
        }

        // --- 新增：浏览路径逻辑 ---
        private void BrowsePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = _cfg.arg?.path ?? "";

            // 如果是选择文件夹，可以使用 FolderBrowserDialog (需要引用 System.Windows.Forms)
            // 这里演示选择文件
            if (dialog.ShowDialog() == true)
            {
                PathBox.Text = dialog.FileName;
                // 立即更新数据
                if (_cfg.arg == null) _cfg.arg = new ButtonConfigNodeArg();
                _cfg.arg.path = dialog.FileName;
                EmitUpdate();
            }
        }

        // --- 新增：名称变更逻辑 ---
        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _cfg.name = NameTextBox.Text;
            EmitUpdate();
        }

        // --- 新增：脚本变更逻辑 ---
        private void ScriptBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_cfg.arg == null) _cfg.arg = new ButtonConfigNodeArg();
            _cfg.arg.script = ScriptBox.Text;
            EmitUpdate();
        }

        // --- 新增：路径变更逻辑 ---
        private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_cfg.arg == null) _cfg.arg = new ButtonConfigNodeArg();
            _cfg.arg.path = PathBox.Text;
            EmitUpdate();
        }

        // --- 新增：添加子节点逻辑 ---
        private void AddChildBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_cfg.useChainLogic) return;
            if (_cfg.logicChain == null) _cfg.logicChain = new List<ButtonConfig>();

            // 创建新节点
            var newChild = new ButtonConfig
            {
                id = ConfigManager.Instance.GetNextId(), // 需确保 ConfigManager 有此方法
                name = "新步骤",
                configType = ExConfigType.Cmd,
                Parent = _cfg
            };

            _cfg.logicChain.Add(newChild);

            // 确保展开状态
            ChildrenContainer.Visibility = Visibility.Visible;
            ExpandBtn.Content = "-";

            RenderChildren();
            EmitUpdate();
        }

        private void ExpandBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ChildrenContainer.Visibility == Visibility.Visible)
            {
                ChildrenContainer.Visibility = Visibility.Collapsed;
                ExpandBtn.Content = "+";
            }
            else
            {
                ChildrenContainer.Visibility = Visibility.Visible;
                ExpandBtn.Content = "-";
                RenderChildren();
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 防止初始化时触发
            if (TypeComboBox.SelectedItem == null || _cfg == null) return;

            if (TypeComboBox.SelectedItem is ExConfigType newType)
            {
                _cfg.configType = newType;

                // 如果变成了 Combine，初始化列表
                if (_cfg.useChainLogic && _cfg.logicChain == null)
                {
                    _cfg.logicChain = new List<ButtonConfig>();
                }

                // 刷新 UI 状态（显示/隐藏编辑框）
                UpdateEditArea();

                // 刷新展开按钮状态
                if (_cfg.useChainLogic)
                {
                    ExpandBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    ExpandBtn.Visibility = Visibility.Collapsed;
                    ChildrenContainer.Visibility = Visibility.Collapsed;
                }

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
    }
}
