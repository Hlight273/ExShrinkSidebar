using ExShrinkSidebar.Script.Localization;
using ExShrinkSidebar.Script.Model;
using Microsoft.Win32;
using System.Windows;

namespace ExShrinkSidebar.UI.Views.EditConfigView
{
    public partial class NodeDetailWindow : Window
    {
        private ButtonConfig _cfg;

        public NodeDetailWindow()
        {
            InitializeComponent();
            CornerBox.ItemsSource = System.Enum.GetValues(typeof(ExWindowCorner));
            ArgumentsLabel.Text = UiTextCatalog.Get(UiTextCatalog.DetailArguments);
            CornerLabel.Text = UiTextCatalog.Get(UiTextCatalog.DetailCorner);
            WidthLabel.Text = UiTextCatalog.Get(UiTextCatalog.DetailWidth);
            HeightLabel.Text = UiTextCatalog.Get(UiTextCatalog.DetailHeight);
            BrowseBtn.Content = UiTextCatalog.Get(UiTextCatalog.DetailBrowse);
            CancelBtn.Content = UiTextCatalog.Get(UiTextCatalog.DetailCancel);
            ConfirmBtn.Content = UiTextCatalog.Get(UiTextCatalog.DetailConfirm);
        }

        public void Init(ButtonConfig cfg)
        {
            _cfg = cfg;
            _cfg.arg ??= new ButtonConfigNodeArg();

            TitleText.Text = $"{cfg.name} {UiTextCatalog.Get(UiTextCatalog.DetailWindowSuffix)}";
            PathBox.Text = _cfg.arg.path ?? string.Empty;
            ArgumentsBox.Text = _cfg.arg.arguments ?? string.Empty;
            CornerBox.SelectedItem = _cfg.arg.windowCorner;
            WidthBox.Text = _cfg.arg.windowWidth > 0 ? _cfg.arg.windowWidth.ToString() : string.Empty;
            HeightBox.Text = _cfg.arg.windowHeight > 0 ? _cfg.arg.windowHeight.ToString() : string.Empty;
            EnablePlacementCheckBox.Content = UiTextCatalog.Get(UiTextCatalog.DetailEnablePlacement);

            SetupLayout();
        }

        private void SetupLayout()
        {
            PathRow.Visibility = Visibility.Collapsed;
            ArgumentsRow.Visibility = Visibility.Collapsed;
            EnablePlacementCheckBox.Visibility = Visibility.Collapsed;
            PlacementRow.Visibility = Visibility.Collapsed;

            switch (_cfg.configType)
            {
                case ExConfigType.OpenFolder:
                    PathLabel.Text = UiTextCatalog.Get(UiTextCatalog.DetailFolderPath);
                    PathRow.Visibility = Visibility.Visible;
                    EnablePlacementCheckBox.Visibility = Visibility.Visible;
                    EnablePlacementCheckBox.IsChecked = _cfg.arg.windowCorner != null;
                    PlacementRow.Visibility = EnablePlacementCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                    HintText.Text = UiTextCatalog.Get(UiTextCatalog.DetailHintOpenFolder);
                    break;
                case ExConfigType.Execute:
                    PathLabel.Text = UiTextCatalog.Get(UiTextCatalog.DetailExecuteTarget);
                    PathRow.Visibility = Visibility.Visible;
                    ArgumentsRow.Visibility = Visibility.Visible;
                    HintText.Text = UiTextCatalog.Get(UiTextCatalog.DetailHintExecute);
                    break;
                default:
                    HintText.Text = UiTextCatalog.Get(UiTextCatalog.DetailHintNone);
                    break;
            }
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_cfg.configType == ExConfigType.OpenFolder)
            {
                using var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.InitialDirectory = PathBox.Text;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    PathBox.Text = dialog.SelectedPath;
                }

                return;
            }

            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = true,
                Filter = UiTextCatalog.Get(UiTextCatalog.FileDialogAllFiles)
            };

            if (fileDialog.ShowDialog() == true)
            {
                PathBox.Text = fileDialog.FileName;
            }
        }

        private void EnablePlacementCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            PlacementRow.Visibility = EnablePlacementCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            _cfg.arg ??= new ButtonConfigNodeArg();
            _cfg.arg.path = PathBox.Text?.Trim() ?? string.Empty;
            _cfg.arg.arguments = ArgumentsBox.Text?.Trim() ?? string.Empty;

            if (EnablePlacementCheckBox.IsChecked == true && CornerBox.SelectedItem is ExWindowCorner corner)
            {
                _cfg.arg.windowCorner = corner;
                _cfg.arg.windowWidth = ParseIntOrZero(WidthBox.Text);
                _cfg.arg.windowHeight = ParseIntOrZero(HeightBox.Text);
            }
            else
            {
                _cfg.arg.windowCorner = null;
                _cfg.arg.windowWidth = 0;
                _cfg.arg.windowHeight = 0;
            }

            DialogResult = true;
            Close();
        }

        private int ParseIntOrZero(string value)
        {
            return int.TryParse(value, out var result) && result > 0 ? result : 0;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
