using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ExShrinkSidebar.UI.Views.MainWindow
{
    public partial class IconButton : System.Windows.Controls.UserControl
    {
        string script;

        public IconButton()
        {
            InitializeComponent();
        }

        public void SetData(string name, string icon, string cmd)
        {
            IconText.Text = name;
            script = cmd;

            if (!string.IsNullOrEmpty(icon))
            {
                IconImage.Source = new BitmapImage(
                    new System.Uri(icon, System.UriKind.RelativeOrAbsolute)
                );
            }
        }

        private void OnClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ScriptRunner.Run(script);
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