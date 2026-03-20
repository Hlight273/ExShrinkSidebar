using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExShrinkSidebar.UI.Views.AddConfigView;

namespace ExShrinkSidebar.UI.Views.MainWindow
{
    /// <summary>
    /// addConfigButton.xaml 的交互逻辑
    /// </summary>
    public partial class AddConfigButton : System.Windows.Controls.UserControl
    {
        // 用于保存窗口引用，防止重复打开
        private AddConfigPanel _configPanelInstance = null;

        public AddConfigButton()
        {
            InitializeComponent();
        }

        private void OnClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 如果窗口已经存在且未关闭，则激活它并返回
            if (_configPanelInstance != null && _configPanelInstance.IsLoaded)
            {
                _configPanelInstance.Activate();
                return;
            }

            // 创建新窗口
            _configPanelInstance = new AddConfigPanel();

            // 设置窗口关闭时清理引用（可选，但推荐）
            _configPanelInstance.Closed += (s, args) =>
            {
                _configPanelInstance = null;
            };

            // 显示窗口
            _configPanelInstance.Show();
        }

        private void OnHover(object sender, System.Windows.Input.MouseEventArgs e)
        {
            RootBorder.Background =
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 255, 255, 255));
        }

        private void OnLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            RootBorder.Background = System.Windows.Media.Brushes.Transparent;
        }
    }
}
