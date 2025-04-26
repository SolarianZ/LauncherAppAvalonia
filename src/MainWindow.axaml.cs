using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using LauncherAppAvalonia.Models;
using LauncherAppAvalonia.Services;
using LauncherAppAvalonia.ViewModels;
using LauncherAppAvalonia.Views;

namespace LauncherAppAvalonia
{
    public partial class MainWindow : Window
    {
        private readonly DataService _dataService;
        private readonly ItemHandlerService _itemHandlerService;
        private readonly LocalizationService _localizationService;
        public MainWindowViewModel ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();

            // 初始化服务
            _dataService = new DataService();
            _itemHandlerService = new ItemHandlerService();
            _localizationService = new LocalizationService();

            // 设置视图模型
            ViewModel = new MainWindowViewModel(_dataService, _itemHandlerService, _localizationService);
            DataContext = ViewModel;
            
            // 传递窗口实例给视图模型
            ViewModel.SetMainWindow(this);

            // 添加事件处理
            AddHandler(DragDrop.DropEvent, OnDropHandler);
            
            // 键盘事件处理
            KeyDown += OnKeyDown;
        }

        /// <summary>
        /// 处理项目双击事件
        /// </summary>
        private void OnItemDoubleTapped(object? sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                ViewModel.OpenItemCommand.Execute(ViewModel.SelectedItem);
            }
        }

        /// <summary>
        /// 处理拖放事件
        /// </summary>
        private async void OnDropHandler(object? sender, DragEventArgs e)
        {
            if (!e.Data.Contains(DataFormats.Files))
                return;

            var files = e.Data.GetFiles();
            if (files == null) 
                return;

            // 转换为List以匹配HandleDroppedItem的参数类型要求
            ViewModel.HandleDroppedItem(files.ToList(), this);
        }

        /// <summary>
        /// 处理键盘事件
        /// </summary>
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            // ESC键关闭窗口
            if (e.Key == Key.Escape)
            {
                Hide();
                e.Handled = true;
            }
            // Delete键移除选中项
            else if (e.Key == Key.Delete && ViewModel.SelectedItem != null)
            {
                ViewModel.RemoveItemCommand.Execute(ViewModel.SelectedItem);
                e.Handled = true;
            }
            // F12键打开开发者工具(在Avalonia中没有直接支持，仅作为占位符)
            else if (e.Key == Key.F12)
            {
                // 在Avalonia中没有内置的开发者工具，这里可以考虑启动Avalonia Inspector
                e.Handled = true;
            }
            // Ctrl+F聚焦到搜索框
            else if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control)
            {
                var searchBox = this.FindControl<TextBox>("SearchBox");
                searchBox?.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 复制文本到剪贴板
        /// </summary>
        public async Task CopyToClipboard(string text)
        {
            await TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(text);
        }

        /// <summary>
        /// 打开编辑项目窗口
        /// </summary>
        public void OpenEditItemWindow(LauncherItemViewModel? item = null)
        {
            var window = new EditItemWindow(_dataService, _itemHandlerService, _localizationService);
            
            if (item != null)
            {
                // 编辑模式
                var index = ViewModel.FilteredItems.IndexOf(item);
                window.SetEditMode(item.ToLauncherItem(), index);
            }
            
            window.ShowDialog(this);
        }

        /// <summary>
        /// 打开设置窗口
        /// </summary>
        public void OpenSettingsWindow()
        {
            var window = new SettingsWindow(_dataService, _localizationService);
            window.ShowDialog(this);
        }
    }
}