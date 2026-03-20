using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ExShrinkSidebar.UI.Views.EditConfigView;

namespace ExShrinkSidebar.UI.Views.MainWindow
{
    public partial class AddConfigButton : System.Windows.Controls.UserControl
    {
        private AddConfigPanel _configPanelInstance = null;

        public AddConfigButton()
        {
            InitializeComponent();
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            if (_configPanelInstance != null && _configPanelInstance.IsLoaded)
            {
                _configPanelInstance.Activate();
                return;
            }

            _configPanelInstance = new AddConfigPanel();
            
            _configPanelInstance.Closed += (s, args) => _configPanelInstance = null;

            // --- 核心修改 ---
            // 传入 null 表示“新建模式”
            // 如果未来有编辑按钮，传入具体的 id 即可，如：_configPanelInstance.Init(123);
            _configPanelInstance.Init(null); 

            _configPanelInstance.Show();
        }

        private void OnHover(object sender, System.Windows.Input.MouseEventArgs e)
        {
            RootBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 255, 255, 255));
        }

        private void OnLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            RootBorder.Background = System.Windows.Media.Brushes.Transparent;
        }
    }
}
