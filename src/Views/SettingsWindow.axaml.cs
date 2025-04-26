using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using LauncherAppAvalonia.Services;
using LauncherAppAvalonia.ViewModels;

namespace LauncherAppAvalonia.Views
{
    public partial class SettingsWindow : Window
    {
        // 修改为可为null或使用required关键字
        private SettingsViewModel? _viewModel;

        public SettingsWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            // 添加键盘事件处理
            KeyDown += OnKeyDown;
        }

        public SettingsWindow(DataService dataService, LocalizationService localizationService) : this()
        {
            _viewModel = new SettingsViewModel(dataService, localizationService, this);
            DataContext = _viewModel;
        }

        /// <summary>
        /// 处理键盘事件
        /// </summary>
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            // ESC键关闭窗口
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            // F12键打开开发者工具
            else if (e.Key == Key.F12)
            {
                // 在Avalonia中，使用AttachDevTools()方法提供开发者工具支持
#if DEBUG
                this.AttachDevTools();
#endif
                e.Handled = true;
            }
        }
    }
}