using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ExShrinkSidebar.Script.Core.Event;
using ExShrinkSidebar.Script.Localization;
using ExShrinkSidebar.UI.Views.EditConfigView;

namespace ExShrinkSidebar.UI.Views.MainWindow
{
    public partial class AddConfigButton : System.Windows.Controls.UserControl
    {
        private AddConfigPanel _configPanelInstance = null;

        public AddConfigButton()
        {
            InitializeComponent();
            IconText.Text = UiTextCatalog.Get(UiTextCatalog.ScriptEditorEntry);
            EVENT.on<LanguageChangedEventArg>(EventIds.ON_LANGUAGE_CHANGED, OnLanguageChanged);
            Unloaded += OnUnloaded;
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
            _configPanelInstance.Init();

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

        private void OnLanguageChanged(object sender, LanguageChangedEventArg e)
        {
            IconText.Text = UiTextCatalog.Get(UiTextCatalog.ScriptEditorEntry);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            EVENT.off<LanguageChangedEventArg>(EventIds.ON_LANGUAGE_CHANGED, OnLanguageChanged);
            Unloaded -= OnUnloaded;
        }
    }
}
