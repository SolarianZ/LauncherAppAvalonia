using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
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

        // 拖拽操作相关变量
        private Point _dragStartPoint;
        private LauncherItemViewModel? _draggedItem;
        private bool _isDragging;

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

            // 注册ListBox拖拽事件
            var itemsListBox = this.FindControl<ListBox>("ItemsListBox");
            if (itemsListBox != null)
            {
                itemsListBox.AddHandler(DragDrop.DragEnterEvent, OnListBoxDragEnter);
                itemsListBox.AddHandler(DragDrop.DragOverEvent, OnListBoxDragOver);
                itemsListBox.AddHandler(DragDrop.DropEvent, OnListBoxDrop);
            }

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
        /// 处理项目指针按下事件（拖拽开始）
        /// </summary>
        private void OnItemPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // 确保只处理左键点击
            if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                if (sender is Grid grid && grid.DataContext is LauncherItemViewModel item)
                {
                    // 记录起始点和拖拽项目
                    _dragStartPoint = e.GetPosition(null);
                    _draggedItem = item;
                    _isDragging = false;

                    // 捕获鼠标 - 修复: 使用InputElement而非Visual
                    var inputElement = (IInputElement)sender;
                    this.PointerMoved += OnItemPointerMoved;
                    this.PointerReleased += OnItemPointerReleased;
                    e.Pointer.Capture(inputElement);
                }
            }
        }

        /// <summary>
        /// 处理项目指针移动事件（拖拽进行中）
        /// </summary>
        private void OnItemPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_draggedItem != null)
            {
                // 计算移动距离
                var currentPos = e.GetPosition(null);
                var delta = currentPos - _dragStartPoint;

                // 如果移动足够距离，开始拖拽
                if (!_isDragging && (Math.Abs(delta.X) > 3 || Math.Abs(delta.Y) > 3))
                {
                    _isDragging = true;

                    // 创建拖拽数据
                    var data = new DataObject();
                    data.Set(DataFormats.Text, _draggedItem.Path);

                    // 开始拖拽操作 - 使用更安全的方式处理任务结果
                    var task = DragDrop.DoDragDrop(e, data, DragDropEffects.Move);

                    // 为了避免警告而不使用ContinueWith，使用同步模式直接等待结果
                    task.ContinueWith(_ =>
                    {
                        // 使用Avalonia的Dispatcher确保UI线程安全
                        Dispatcher.UIThread.Post(() =>
                        {
                            // 拖拽完成，释放捕获
                            this.PointerMoved -= OnItemPointerMoved;
                            this.PointerReleased -= OnItemPointerReleased;
                            e.Pointer.Capture(null);
                        });
                    }, TaskScheduler.Default);
                }
            }
        }

        /// <summary>
        /// 处理项目指针释放事件（拖拽结束）
        /// </summary>
        private void OnItemPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            // 移除Visual类型检查，直接释放事件和捕获
            this.PointerMoved -= OnItemPointerMoved;
            this.PointerReleased -= OnItemPointerReleased;
            e.Pointer.Capture(null);

            _isDragging = false;
            _draggedItem = null;
        }

        /// <summary>
        /// 处理ListBox拖拽进入事件
        /// </summary>
        private void OnListBoxDragEnter(object? sender, DragEventArgs e)
        {
            // 判断是内部拖拽还是外部文件拖入
            if (_isDragging && _draggedItem != null)
            {
                // 内部拖拽排序
                e.DragEffects = DragDropEffects.Move;
            }
            else if (e.Data.Contains(DataFormats.Files))
            {
                // 外部文件拖入
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        /// <summary>
        /// 处理ListBox拖拽悬停事件
        /// </summary>
        private void OnListBoxDragOver(object? sender, DragEventArgs e)
        {
            // 与DragEnter逻辑相同
            OnListBoxDragEnter(sender, e);
        }

        /// <summary>
        /// 处理ListBox拖拽放下事件
        /// </summary>
        private void OnListBoxDrop(object? sender, DragEventArgs e)
        {
            // 处理内部排序拖拽
            if (_isDragging && _draggedItem != null && sender is ListBox listBox)
            {
                var targetPos = e.GetPosition(listBox);
                var targetItem = GetItemAt(listBox, targetPos);

                if (targetItem != null && !targetItem.Equals(_draggedItem))
                {
                    // 获取源和目标索引
                    var sourceIdx = ViewModel.FilteredItems.IndexOf(_draggedItem);
                    var targetIdx = ViewModel.FilteredItems.IndexOf(targetItem);

                    if (sourceIdx != -1 && targetIdx != -1)
                    {
                        // 移动项目
                        MoveItem(sourceIdx, targetIdx);
                    }
                }

                _isDragging = false;
                _draggedItem = null;
                e.Handled = true;
            }
        }

        /// <summary>
        /// 获取列表中指定位置的项目
        /// </summary>
        private LauncherItemViewModel? GetItemAt(ListBox listBox, Point point)
        {
            // 使用Avalonia的ItemContainerGenerator机制获取项目
            foreach (var item in listBox.ItemsSource)
            {
                if (item is LauncherItemViewModel viewModel)
                {
                    var container = listBox.ContainerFromItem(item) as Control;
                    if (container != null)
                    {
                        var bounds = container.Bounds;
                        var relativePoint = point - bounds.Position;

                        if (bounds.Contains(relativePoint))
                        {
                            return viewModel;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 移动项目顺序
        /// </summary>
        private void MoveItem(int sourceIndex, int targetIndex)
        {
            if (sourceIndex == targetIndex)
                return;

            // 在ViewModel中执行移动
            var items = ViewModel.FilteredItems.ToList();
            var item = items[sourceIndex];

            // 移除原始位置的项目
            items.RemoveAt(sourceIndex);

            // 插入到目标位置
            if (targetIndex > sourceIndex) targetIndex--;
            items.Insert(targetIndex, item);

            // 更新数据
            ViewModel.UpdateItemsOrder(items);
        }

        /// <summary>
        /// 处理拖放事件
        /// </summary>
        private void OnDropHandler(object? sender, DragEventArgs e)
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
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(text);
            }
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