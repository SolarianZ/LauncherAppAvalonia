using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.Services
{
    /// <summary>
    /// 系统托盘服务，负责创建和管理系统托盘图标
    /// </summary>
    public class TrayService : IDisposable
    {
        private readonly DataService _dataService;
        private readonly ItemHandlerService _itemHandlerService;
        private readonly LocalizationService _localizationService;

        // 托盘图标实例（根据平台可能使用不同的实现）
        private object? _trayIcon;

        // 显示主窗口的回调
        private Action? _showMainWindow;

        // 最近使用项目的最大数量
        private const int MaxRecentItems = 8;

        public TrayService(DataService dataService, ItemHandlerService itemHandlerService,
            LocalizationService localizationService)
        {
            _dataService = dataService;
            _itemHandlerService = itemHandlerService;
            _localizationService = localizationService;

            // 监听项目变更事件，以便更新托盘菜单
            _dataService.ItemsChanged += OnItemsChanged;
        }

        /// <summary>
        /// 创建系统托盘图标
        /// </summary>
        /// <param name="showMainWindowAction">显示主窗口的回调</param>
        public void CreateTrayIcon(Action showMainWindowAction)
        {
            _showMainWindow = showMainWindowAction;

            // 根据平台选择实现
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateWindowsTrayIcon();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                CreateMacTrayIcon();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                CreateLinuxTrayIcon();
            }
        }

        /// <summary>
        /// 清理托盘图标资源
        /// </summary>
        public void Dispose()
        {
            DisposeTrayIcon();
            _trayIcon = null;
        }

        /// <summary>
        /// 处理项目变更事件
        /// </summary>
        private void OnItemsChanged(object? sender, EventArgs e)
        {
            // 更新托盘菜单
            UpdateTrayMenu();
        }

        /// <summary>
        /// 更新托盘菜单
        /// </summary>
        private void UpdateTrayMenu()
        {
            if (_trayIcon == null) return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UpdateWindowsTrayMenu();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                UpdateMacTrayMenu();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                UpdateLinuxTrayMenu();
            }
        }

        /// <summary>
        /// 创建 Windows 平台的托盘图标
        /// </summary>
        private void CreateWindowsTrayIcon()
        {
#if WINDOWS
            try
            {
                // 获取托盘图标路径
                string iconPath = GetTrayIconPath();
                
                // 初始化托盘图标
                var notifyIcon = new System.Windows.Forms.NotifyIcon
                {
                    Icon = new System.Drawing.Icon(iconPath),
                    Text = "Launcher App",
                    Visible = true
                };
                
                // 设置双击事件
                notifyIcon.DoubleClick += (s, e) => _showMainWindow?.Invoke();
                
                // 创建初始菜单
                UpdateWindowsTrayMenu(notifyIcon);
                
                _trayIcon = notifyIcon;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Windows tray icon: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// 更新 Windows 托盘菜单
        /// </summary>
        private void UpdateWindowsTrayMenu()
        {
#if WINDOWS
            if (_trayIcon is System.Windows.Forms.NotifyIcon notifyIcon)
            {
                UpdateWindowsTrayMenu(notifyIcon);
            }
#endif
        }

#if WINDOWS
        /// <summary>
        /// 更新 Windows 托盘菜单实现
        /// </summary>
        private void UpdateWindowsTrayMenu(System.Windows.Forms.NotifyIcon notifyIcon)
        {
            try
            {
                // 创建上下文菜单
                var contextMenu = new System.Windows.Forms.ContextMenuStrip();
                
                // 添加最近使用的项目
                var recentItems = GetRecentItems();
                if (recentItems.Count > 0)
                {
                    foreach (var item in recentItems)
                    {
                        var displayName = !string.IsNullOrEmpty(item.Name) 
                            ? item.Name 
                            : Path.GetFileName(item.Path) ?? item.Path;

                        string iconText = GetItemIconText(item.Type);
                        var menuItem = new System.Windows.Forms.ToolStripMenuItem($"{iconText} {displayName}");
                        
                        // 捕获item变量，以便在事件处理程序中使用
                        var capturedItem = item;
                        menuItem.Click += (s, e) => _itemHandlerService.HandleItemAction(capturedItem);
                        contextMenu.Items.Add(menuItem);
                    }
                    
                    // 添加分隔符
                    contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
                }
                
                // 添加"打开主窗口"菜单项
                var openMenuItem = new System.Windows.Forms.ToolStripMenuItem("打开启动器");
                openMenuItem.Click += (s, e) => _showMainWindow?.Invoke();
                contextMenu.Items.Add(openMenuItem);
                
                // 添加"退出"菜单项
                var exitMenuItem = new System.Windows.Forms.ToolStripMenuItem("退出");
                exitMenuItem.Click += (s, e) => 
                {
                    notifyIcon.Visible = false;
                    Environment.Exit(0);
                };
                contextMenu.Items.Add(exitMenuItem);
                
                // 设置上下文菜单
                notifyIcon.ContextMenuStrip = contextMenu;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Windows tray menu: {ex.Message}");
            }
        }
#endif

        /// <summary>
        /// 创建 macOS 平台的托盘图标
        /// </summary>
        private void CreateMacTrayIcon()
        {
            // macOS平台使用第三方库或原生API调用实现
            // 这里使用状态菜单来模拟托盘菜单

            try
            {
                // 对于跨平台应用，我们可以使用多种方式实现macOS的托盘图标
                // 1. 使用MonoMac或Xamarin.Mac (需要特定依赖)
                // 2. 使用Objective-C互操作调用NSStatusBar API
                // 3. 使用第三方跨平台托盘库

                // 由于这些实现需要特定库的支持，这里先打印一条消息
                Console.WriteLine("Creating macOS tray icon (mock implementation)");

                // 如果有合适的依赖，可以在这里实现真正的macOS托盘
                _trayIcon = new object(); // 临时占位对象
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating macOS tray icon: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新 macOS 托盘菜单
        /// </summary>
        private void UpdateMacTrayMenu()
        {
            // macOS平台托盘菜单更新逻辑
            Console.WriteLine("Updating macOS tray menu (mock implementation)");
        }

        /// <summary>
        /// 创建 Linux 平台的托盘图标
        /// </summary>
        private void CreateLinuxTrayIcon()
        {
            // Linux平台通常使用AppIndicator或libnotify实现托盘功能
            try
            {
                Console.WriteLine("Creating Linux tray icon (mock implementation)");

                // Linux托盘图标实现需要GTK或Qt的支持，这里先占位
                _trayIcon = new object(); // 临时占位对象

                // TODO: 实现实际的Linux托盘图标，可能需要使用D-Bus或AppIndicator
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Linux tray icon: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新 Linux 托盘菜单
        /// </summary>
        private void UpdateLinuxTrayMenu()
        {
            // Linux平台托盘菜单更新逻辑
            Console.WriteLine("Updating Linux tray menu (mock implementation)");
        }

        /// <summary>
        /// 释放托盘图标资源
        /// </summary>
        private void DisposeTrayIcon()
        {
            if (_trayIcon == null) return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
#if WINDOWS
                if (_trayIcon is System.Windows.Forms.NotifyIcon notifyIcon)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }
#endif
            }
        }

        /// <summary>
        /// 获取托盘图标路径
        /// </summary>
        private string GetTrayIconPath()
        {
            // 默认图标路径
            string defaultIconPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets", "Icons", "tray-icon.png");

            // 检查是否存在
            if (!File.Exists(defaultIconPath))
            {
                // 尝试查找可能的替代路径
                string altIconPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Assets", "Icons", "app-icon.ico");

                if (File.Exists(altIconPath))
                {
                    return altIconPath;
                }

                throw new FileNotFoundException("Tray icon not found");
            }

            return defaultIconPath;
        }

        /// <summary>
        /// 获取项目类型对应的图标文本
        /// </summary>
        private string GetItemIconText(PathType type)
        {
            return type switch
            {
                PathType.File => "📄",
                PathType.Folder => "📁",
                PathType.Url => "🌐",
                PathType.Command => "⌨️",
                _ => "❓"
            };
        }

        /// <summary>
        /// 获取最近使用的项目列表
        /// </summary>
        private List<LauncherItem> GetRecentItems()
        {
            var items = _dataService.GetItems();

            // 排序（按最后访问时间降序）
            items.Sort((a, b) => b.LastAccessed.CompareTo(a.LastAccessed));

            // 限制数量
            if (items.Count > MaxRecentItems)
            {
                items = items.GetRange(0, MaxRecentItems);
            }

            return items;
        }
    }
}