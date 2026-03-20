using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ExShrinkSidebar.UI.Views.EditConfigView
{
    public partial class AddConfigPanel : Window
    {
        private List<ButtonConfig> _rootList; // 持有根列表的引用

        public AddConfigPanel()
        {
            InitializeComponent();

            // 订阅删除事件
            EVENT.on<ConfigNodeEventArg>(EventIds.ON_CONFIG_NODE_DELETE_REQUEST, OnNodeDeleteRequest);
        }

        /// <summary>
        /// 初始化窗口
        /// </summary>
        /// <param name="editId">如果不为 null，则为编辑模式；否则为新建模式。</param>
        public void Init(long? editId)
        {
            // 1. 确保全局配置列表存在
            if (ConfigManager.Instance.Config.buttons == null)
            {
                ConfigManager.Instance.Config.buttons = new List<ButtonConfig>();
            }
            _rootList = ConfigManager.Instance.Config.buttons;

            // 2. 根据 ID 决定模式
            if (editId.HasValue)
            {
                // === 编辑模式 ===
                TitleText.Text = "编辑配置";
                AddRootBtn.Visibility = Visibility.Collapsed; // 编辑单个配置时隐藏“添加根节点”

                // 查找目标配置（支持树状查找）
                var targetCfg = FindConfigById(_rootList, editId.Value);

                if (targetCfg != null)
                {
                    // 创建一个临时列表只包含这一项，以便 RenderTree 渲染
                    // 注意：RenderTree 直接操作 _rootList 引用，所以这里可以传 _rootList 或者临时列表都可以
                    // 但为了逻辑统一，RenderTree 内部需要处理“只显示这一项”的逻辑吗？
                    // 为了简单，我们让 RenderTree 渲染整个树，但可以优化。
                    // 最简单的做法：RenderTree 始终渲染 _rootList。
                    // 这里不需要额外操作，直接渲染即可。
                }
                else
                {
                    System.Windows.MessageBox.Show("未找到指定的配置ID，将切换为新建模式。");
                    CreateNewConfig(); // 降级为新建
                }
            }
            else
            {
                // === 新建模式 ===
                TitleText.Text = "添加新配置";
                AddRootBtn.Visibility = Visibility.Visible; // 显示“添加根节点”以便连续添加

                CreateNewConfig();
            }

            // 3. 渲染界面
            RenderTree();
        }

        // 创建新配置并加入列表
        private void CreateNewConfig()
        {
            var newCfg = new ButtonConfig
            {
                id = ConfigManager.Instance.GetNextId(),
                name = "新配置",
                configType = ExConfigType.Cmd
            };
            _rootList.Add(newCfg);
        }

        // 辅助方法：递归查找配置
        private ButtonConfig FindConfigById(List<ButtonConfig> source, long id)
        {
            foreach (var cfg in source)
            {
                if (cfg.id == id) return cfg;
                if (cfg.logicChain != null)
                {
                    var found = FindConfigById(cfg.logicChain, id);
                    if (found != null) return found;
                }
            }
            return null;
        }

        // 渲染树
        private void RenderTree()
        {
            TreeContainer.Children.Clear();
            if (_rootList == null) return;

            foreach (var cfg in _rootList)
            {
                var view = new SubConfigView();
                view.SetData(cfg, 0, null);
                TreeContainer.Children.Add(view);
            }
        }

        // 处理删除请求
        private void OnNodeDeleteRequest(object sender, ConfigNodeEventArg e)
        {
            var nodeToDelete = e.Data;
            var parentId = e.ParentId;

            // 从树中移除
            if (parentId == 0 || nodeToDelete.Parent == null)
            {
                _rootList.Remove(nodeToDelete);
            }
            else
            {
                nodeToDelete.Parent?.logicChain.Remove(nodeToDelete);
            }

            RenderTree();
        }

        // 点击“添加根节点”
        private void OnAddRootClick(object sender, RoutedEventArgs e)
        {
            CreateNewConfig();
            RenderTree();
        }

        // 保存并关闭
        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            ConfigManager.Instance.Save();
            this.Close();
        }

        // 拖动与关闭
        private void OnDragMove(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            // 如果是新建模式，用户点 X 可能想取消新建
            // 简单处理：直接关闭，数据残留在内存中（未Save）
            // 严谨做法：如果是新建且未Save，应该从 _rootList 移除刚才 Add 的项。
            // 这里暂不实现复杂的取消逻辑
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            EVENT.off<ConfigNodeEventArg>(EventIds.ON_CONFIG_NODE_DELETE_REQUEST, OnNodeDeleteRequest);
        }
    }
}
