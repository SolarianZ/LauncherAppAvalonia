using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LauncherAppAvalonia.Models;
using LauncherAppAvalonia.Services;

namespace LauncherAppAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly DataService _dataService;
        private readonly ItemHandlerService _itemHandlerService;
        private readonly LocalizationService _localizationService;
        private string _searchText = string.Empty;
        private LauncherItemViewModel? _selectedItem;
        private ObservableCollection<LauncherItemViewModel> _items = new();
        private ObservableCollection<LauncherItemViewModel> _filteredItems = new();
        private MainWindow? _mainWindow;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterItems();
                }
            }
        }

        public ObservableCollection<LauncherItemViewModel> FilteredItems
        {
            get => _filteredItems;
            private set => SetProperty(ref _filteredItems, value);
        }

        public LauncherItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        // 命令
        public ICommand AddItemCommand { get; }
        public ICommand OpenItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ShowInFolderCommand { get; }
        public ICommand CopyPathCommand { get; }
        public ICommand EditItemCommand { get; }

        public MainWindowViewModel(DataService dataService, ItemHandlerService itemHandlerService, LocalizationService localizationService)
        {
            _dataService = dataService;
            _itemHandlerService = itemHandlerService;
            _localizationService = localizationService;

            // 注册数据变更事件
            _dataService.ItemsChanged += OnItemsChanged;

            // 初始化命令
            AddItemCommand = new RelayCommand(AddItem);
            OpenItemCommand = new RelayCommand<LauncherItemViewModel>(OpenItem, CanExecuteItemCommand);
            RemoveItemCommand = new RelayCommand<LauncherItemViewModel>(RemoveItem, CanExecuteItemCommand);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            ShowInFolderCommand = new RelayCommand<LauncherItemViewModel>(ShowInFolder, CanShowInFolder);
            CopyPathCommand = new RelayCommand<LauncherItemViewModel>(CopyPath, CanExecuteItemCommand);
            EditItemCommand = new RelayCommand<LauncherItemViewModel>(EditItem, CanExecuteItemCommand);

            // 加载项目列表
            LoadItems();
        }

        /// <summary>
        /// 设置主窗口引用，用于打开子窗口
        /// </summary>
        public void SetMainWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// 加载项目列表
        /// </summary>
        private void LoadItems()
        {
            var items = _dataService.GetItems();
            _items.Clear();
            
            foreach (var item in items)
            {
                _items.Add(new LauncherItemViewModel(item));
            }
            
            FilterItems();
        }

        /// <summary>
        /// 筛选项目
        /// </summary>
        private void FilterItems()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredItems = new ObservableCollection<LauncherItemViewModel>(_items);
            }
            else
            {
                var query = SearchText.ToLower();
                var filtered = _items.Where(item => 
                    (item.Name?.ToLower().Contains(query) ?? false) || 
                    item.Path.ToLower().Contains(query)).ToList();
                
                FilteredItems = new ObservableCollection<LauncherItemViewModel>(filtered);
            }
        }

        /// <summary>
        /// 项目变更事件处理
        /// </summary>
        private void OnItemsChanged(object? sender, EventArgs e)
        {
            LoadItems();
        }

        /// <summary>
        /// 添加项目
        /// </summary>
        private void AddItem()
        {
            if (_mainWindow == null) return;
            _mainWindow.OpenEditItemWindow();
        }

        /// <summary>
        /// 编辑项目
        /// </summary>
        private void EditItem(LauncherItemViewModel? item)
        {
            if (item == null || _mainWindow == null) return;
            _mainWindow.OpenEditItemWindow(item);
        }

        /// <summary>
        /// 打开项目
        /// </summary>
        private void OpenItem(LauncherItemViewModel? item)
        {
            if (item == null) return;
            
            _itemHandlerService.HandleItemAction(item.ToLauncherItem());
        }

        /// <summary>
        /// 移除项目
        /// </summary>
        private void RemoveItem(LauncherItemViewModel? item)
        {
            if (item == null) return;
            
            int index = _items.IndexOf(item);
            if (index >= 0)
            {
                _dataService.RemoveItem(index);
                LoadItems();
            }
        }

        /// <summary>
        /// 打开设置
        /// </summary>
        private void OpenSettings()
        {
            if (_mainWindow == null) return;
            _mainWindow.OpenSettingsWindow();
        }

        /// <summary>
        /// 在文件夹中显示项目
        /// </summary>
        private void ShowInFolder(LauncherItemViewModel? item)
        {
            if (item == null) return;
            
            _itemHandlerService.ShowItemInFolder(item.Path);
        }

        /// <summary>
        /// 复制路径
        /// </summary>
        private async void CopyPath(LauncherItemViewModel? item)
        {
            if (item == null || _mainWindow == null) return;
            
            await _mainWindow.CopyToClipboard(item.Path);
            // TODO: 显示提示信息 "路径已复制"
        }

        /// <summary>
        /// 检查是否可以显示在文件夹中
        /// </summary>
        private bool CanShowInFolder(LauncherItemViewModel? item)
        {
            return item != null && (item.Type == PathType.File || item.Type == PathType.Folder);
        }

        /// <summary>
        /// 检查项目命令是否可执行
        /// </summary>
        private bool CanExecuteItemCommand(LauncherItemViewModel? item)
        {
            return item != null;
        }

        /// <summary>
        /// 更新项目排序
        /// </summary>
        public void UpdateItemsOrder()
        {
            var items = _items.Select(vm => vm.ToLauncherItem()).ToList();
            _dataService.UpdateItemsOrder(items);
        }

        /// <summary>
        /// 处理拖放项目
        /// </summary>
        public async void HandleDroppedItem(IReadOnlyList<IStorageItem> items, TopLevel topLevel)
        {
            if (items.Count == 0) return;

            foreach (var storageItem in items)
            {
                string path = storageItem.Path.LocalPath;
                PathType type = _itemHandlerService.GetItemType(path);
                string name = System.IO.Path.GetFileName(path);

                var item = new LauncherItem(path, type, name);
                _dataService.AddItem(item);
            }
        }
    }

    /// <summary>
    /// 项目视图模型
    /// </summary>
    public class LauncherItemViewModel : ViewModelBase
    {
        public string Path { get; }
        public PathType Type { get; }
        public string? Name { get; }
        public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : System.IO.Path.GetFileName(Path) ?? Path;

        public LauncherItemViewModel(LauncherItem item)
        {
            Path = item.Path;
            Type = item.Type;
            Name = item.Name;
        }

        public LauncherItem ToLauncherItem()
        {
            return new LauncherItem(Path, Type, Name);
        }

        public string GetIcon()
        {
            return Type switch
            {
                PathType.File => "📄",
                PathType.Folder => "📁",
                PathType.Url => "🌐",
                PathType.Command => "⌨️",
                _ => "❓"
            };
        }
    }
}