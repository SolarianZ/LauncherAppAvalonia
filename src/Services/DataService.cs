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
        }

        /// <summary>
        /// 更新语言配置
        /// </summary>
        public void UpdateLanguageConfig(string language)
        {
            _config.Language = language;
            SaveConfig();
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
        /// 更新自动启动配置
        /// </summary>
        public void UpdateAutoLaunchConfig(bool enabled)
        {
            _config.AutoLaunch.Enabled = enabled;
            SaveConfig();
        }
    }

    /// <summary>
    /// 应用配置类
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// 主窗口配置
        /// </summary>
        public MainWindowConfig MainWindow { get; set; } = new();

        /// <summary>
        /// 主题配置
        /// </summary>
        public string Theme { get; set; } = "system";

        /// <summary>
        /// 语言配置
        /// </summary>
        public string Language { get; set; } = "system";

        /// <summary>
        /// 快捷键配置
        /// </summary>
        public ShortcutConfig Shortcut { get; set; } = new();

        /// <summary>
        /// 自动启动配置
        /// </summary>
        public AutoLaunchConfig AutoLaunch { get; set; } = new();
    }

    /// <summary>
    /// 主窗口配置类
    /// </summary>
    public class MainWindowConfig
    {
        public double Width { get; set; } = 400;
        public double Height { get; set; } = 600;
        public double X { get; set; } = double.NaN;
        public double Y { get; set; } = double.NaN;
    }

    /// <summary>
    /// 快捷键配置类
    /// </summary>
    public class ShortcutConfig
    {
        public bool Enabled { get; set; } = true;
        public string Shortcut { get; set; } = "Alt+Shift+Q";
    }

    /// <summary>
    /// 自动启动配置类
    /// </summary>
    public class AutoLaunchConfig
    {
        public bool Enabled { get; set; } = false;
    }

    /// <summary>
    /// 快捷键配置事件参数
    /// </summary>
    public class ShortcutConfigArgs : EventArgs
    {
        public ShortcutConfig Config { get; }

        public ShortcutConfigArgs(ShortcutConfig config)
        {
            Config = config;
        }
    }
}