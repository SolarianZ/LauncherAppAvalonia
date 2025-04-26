using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.Services
{
    /// <summary>
    /// 数据服务，负责应用数据的持久化存储和管理
    /// </summary>
    public class DataService
    {
        // 文件路径常量
        private readonly string _userDataFolder;
        private readonly string _itemsFilePath;
        private readonly string _configFilePath;

        // 应用数据
        private List<LauncherItem> _items = new();
        private AppConfig _config = new();

        // 数据变更事件
        public event EventHandler? ItemsChanged;
        public event EventHandler<ShortcutConfigArgs>? ShortcutConfigChanged;
        public event EventHandler<string>? ThemeChanged;

        public DataService()
        {
            // 获取用户数据目录路径
            _userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LauncherAppAvalonia",
                "UserData");

            // 确保目录存在
            if (!Directory.Exists(_userDataFolder))
            {
                Directory.CreateDirectory(_userDataFolder);
            }

            // 设置文件路径
            _itemsFilePath = Path.Combine(_userDataFolder, "items.json");
            _configFilePath = Path.Combine(_userDataFolder, "configs.json");

            // 加载数据
            LoadItems();
            LoadConfig();
        }

        /// <summary>
        /// 加载项目列表
        /// </summary>
        public void LoadItems()
        {
            try
            {
                if (File.Exists(_itemsFilePath))
                {
                    string json = File.ReadAllText(_itemsFilePath);
                    var items = JsonSerializer.Deserialize<List<LauncherItem>>(json);
                    if (items != null)
                    {
                        _items = items;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading items: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存项目列表
        /// </summary>
        public void SaveItems()
        {
            try
            {
                string json = JsonSerializer.Serialize(_items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_itemsFilePath, json);

                // 触发项目变更事件
                ItemsChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving items: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取项目列表
        /// </summary>
        public List<LauncherItem> GetItems()
        {
            return new List<LauncherItem>(_items);
        }

        /// <summary>
        /// 添加项目
        /// </summary>
        public void AddItem(LauncherItem item)
        {
            _items.Add(item);
            SaveItems();
        }

        /// <summary>
        /// 更新项目
        /// </summary>
        public void UpdateItem(int index, LauncherItem item)
        {
            if (index >= 0 && index < _items.Count)
            {
                _items[index] = item;
                SaveItems();
            }
        }

        /// <summary>
        /// 移除项目
        /// </summary>
        public void RemoveItem(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                _items.RemoveAt(index);
                SaveItems();
            }
        }

        /// <summary>
        /// 更新项目顺序
        /// </summary>
        public void UpdateItemsOrder(List<LauncherItem> items)
        {
            _items = new List<LauncherItem>(items);
            SaveItems();
        }

        /// <summary>
        /// 清空所有项目
        /// </summary>
        public void ClearAllItems()
        {
            _items.Clear();
            SaveItems();
        }

        /// <summary>
        /// 加载应用配置
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string json = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null)
                    {
                        _config = config;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存应用配置
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取应用配置
        /// </summary>
        public AppConfig GetConfig()
        {
            return _config;
        }

        /// <summary>
        /// 更新主题配置
        /// </summary>
        public void UpdateThemeConfig(string theme)
        {
            _config.Theme = theme;
            SaveConfig();

            // 触发主题变更事件
            ThemeChanged?.Invoke(this, theme);
        }

        /// <summary>
        /// 更新语言配置
        /// </summary>
        public void UpdateLanguageConfig(string language)
        {
            _config.Language = language;
            SaveConfig();

            // 语言变更不在这里触发事件，因为需要通过LocalizationService来处理
        }

        /// <summary>
        /// 更新快捷键配置
        /// </summary>
        public void UpdateShortcutConfig(ShortcutConfig config)
        {
            _config.Shortcut = config;
            SaveConfig();

            // 触发快捷键配置变更事件
            ShortcutConfigChanged?.Invoke(this, new ShortcutConfigArgs(_config.Shortcut));
        }

        /// <summary>
        /// 更新启动行为配置
        /// </summary>
        public void UpdateStartupConfig(bool minimizeToTray)
        {
            _config.MinimizeToTrayOnStart = minimizeToTray;
            SaveConfig();
        }

        /// <summary>
        /// 更新关闭行为配置
        /// </summary>
        public void UpdateCloseConfig(bool minimizeToTray)
        {
            _config.MinimizeToTrayOnClose = minimizeToTray;
            SaveConfig();
        }

        /// <summary>
        /// 更新更新检查配置
        /// </summary>
        public void UpdateCheckUpdateConfig(bool checkOnStart)
        {
            _config.CheckUpdateOnStart = checkOnStart;
            SaveConfig();
        }

        /// <summary>
        /// 更新自动启动配置
        /// </summary>
        public void UpdateAutoLaunchConfig(bool enabled)
        {
            _config.AutoLaunch.Enabled = enabled;
            SaveConfig();
            
            // 根据平台实现开机自启动配置
            SetSystemAutoStart(enabled);
        }

        /// <summary>
        /// 设置系统开机自启动
        /// </summary>
        private void SetSystemAutoStart(bool enabled)
        {
            try
            {
                // 不同平台有不同的实现方式，这里仅作为示例
                if (OperatingSystem.IsWindows())
                {
                    // Windows平台使用注册表实现开机自启动
                    Console.WriteLine($"Setting Windows auto start: {enabled}");
                    // 实际实现需要使用Windows注册表API
                }
                else if (OperatingSystem.IsMacOS())
                {
                    // macOS平台使用LaunchAgent实现开机自启动
                    Console.WriteLine($"Setting macOS auto start: {enabled}");
                    // 实际实现需要创建或删除plist文件
                }
                else if (OperatingSystem.IsLinux())
                {
                    // Linux平台使用自启动文件实现
                    Console.WriteLine($"Setting Linux auto start: {enabled}");
                    // 实际实现需要创建或删除.desktop文件
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting auto start: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取数据存储位置
        /// </summary>
        public string GetDataFolderPath()
        {
            return _userDataFolder;
        }
    }
}