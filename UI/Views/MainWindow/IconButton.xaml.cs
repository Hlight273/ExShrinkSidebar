using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Model;
using ExShrinkSidebar.Script.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ExShrinkSidebar.UI.Views.MainWindow
{
    public partial class IconButton : System.Windows.Controls.UserControl
    {
        private ButtonConfig _cfg;

        public IconButton()
        {
            InitializeComponent();
        }

        public void SetData(ButtonConfig cfg)
        {
            _cfg = cfg;
            IconText.Text = cfg.name;
            IconImage.Source = new BitmapImage(new Uri( ResHelper.GetExConfigTypeIconPath(cfg)));
        }

        private void OnClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnCfgLogicRunner.Run(_cfg);
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