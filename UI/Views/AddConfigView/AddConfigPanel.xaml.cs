using ExShrinkSidebar.Script.Core;
using ExShrinkSidebar.Script.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExShrinkSidebar.UI.Views.AddConfigView
{
    public partial class AddConfigPanel : Window
    {
        public AddConfigPanel()
        {
            InitializeComponent();
            // 加载枚举到下拉框
            TypeComboBox.ItemsSource = Enum.GetValues(typeof(ExConfigType));
            TypeComboBox.SelectedIndex = 0;
        }

        // 允许拖动窗口
        private void OnDragMove(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            // 获取选中的类型
            if (TypeComboBox.SelectedItem is ExConfigType selectedType)
            {
                // 获取名称，如果为空则使用默认值
                string name = string.IsNullOrWhiteSpace(NameTextBox.Text) ? "未命名" : NameTextBox.Text;

                // TODO: 在此处处理保存逻辑
                // 例如: ConfigManager.Add(new ConfigItem { Type = selectedType, Name = name });

                System.Windows.MessageBox.Show($"已添加配置：{name} ({selectedType})");
            }

            // 关闭窗口
            this.Close();
        }
    }
}
