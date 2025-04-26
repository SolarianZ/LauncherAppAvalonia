using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using LauncherAppAvalonia.Models;
using LauncherAppAvalonia.Services;
using LauncherAppAvalonia.ViewModels;

namespace LauncherAppAvalonia.Views
{
    public partial class EditItemWindow : Window
    {
        // 修改为可为null或使用required关键字
        private EditItemViewModel? _viewModel;

        public EditItemWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            // 键盘事件监听
            KeyDown += OnKeyDown;
        }

        public EditItemWindow(DataService dataService, ItemHandlerService itemHandlerService, LocalizationService localizationService) : this()
        {
            _viewModel = new EditItemViewModel(dataService, itemHandlerService, localizationService, this);
            DataContext = _viewModel;
        }
        
        /// <summary>
        /// 设置为编辑模式
        /// </summary>
        public void SetEditMode(LauncherItem item, int index)
        {
            // 添加空值检查以避免空引用异常
            _viewModel?.SetEditMode(item, index);
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
        }
    }
}