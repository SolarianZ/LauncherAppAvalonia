using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
using LauncherAppAvalonia.Models;
using LauncherAppAvalonia.Services;

namespace LauncherAppAvalonia.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly DataService _dataService;
        private readonly LocalizationService _localizationService;
        private readonly Window _parentWindow;
        
        private string _selectedTheme;
        private string _selectedLanguage;
        private bool _enableShortcut;
        private string _shortcut;
        private bool _enableAutoLaunch;
        private string _appVersion = "1.0.0";
        private List<string> _availableLanguages = new();
        private List<string> _availableThemes = new();
        
        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    UpdateTheme(value);
                }
            }
        }
        
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value))
                {
                    UpdateLanguage(value);
                }
            }
        }
        
        public bool EnableShortcut
        {
            get => _enableShortcut;
            set
            {
                if (SetProperty(ref _enableShortcut, value))
                {
                    UpdateShortcutConfig();
                }
            }
        }
        
        public string Shortcut
        {
            get => _shortcut;
            set
            {
                if (SetProperty(ref _shortcut, value))
                {
                    UpdateShortcutConfig();
                }
            }
        }
        
        public bool EnableAutoLaunch
        {
            get => _enableAutoLaunch;
            set
            {
                if (SetProperty(ref _enableAutoLaunch, value))
                {
                    UpdateAutoLaunch(value);
                }
            }
        }
        
        public string AppVersion
        {
            get => _appVersion;
            set => SetProperty(ref _appVersion, value);
        }
        
        public List<string> AvailableLanguages
        {
            get => _availableLanguages;
            set => SetProperty(ref _availableLanguages, value);
        }
        
        public List<string> AvailableThemes
        {
            get => _availableThemes;
            set => SetProperty(ref _availableThemes, value);
        }
        
        // 命令
        public ICommand OpenStorageLocationCommand { get; }
        public ICommand ClearDataCommand { get; }
        public ICommand OpenGithubCommand { get; }
        public ICommand ReportIssueCommand { get; }
        
        public SettingsViewModel(DataService dataService, LocalizationService localizationService, Window parentWindow)
        {
            _dataService = dataService;
            _localizationService = localizationService;
            _parentWindow = parentWindow;
            
            // 初始化可用主题列表
            _availableThemes = new List<string> { "system", "light", "dark" };
            _selectedTheme = _dataService.GetConfig().Theme;
            
            // 初始化可用语言列表
            _availableLanguages = _localizationService.GetAvailableLanguages();
            _selectedLanguage = _localizationService.GetCurrentLanguage();
            
            // 初始化快捷键配置
            var shortcutConfig = _dataService.GetConfig().Shortcut;
            _enableShortcut = shortcutConfig.Enabled;
            _shortcut = shortcutConfig.Shortcut;
            
            // 初始化自启动配置
            _enableAutoLaunch = _dataService.GetConfig().AutoLaunch.Enabled;
            
            // 初始化命令
            OpenStorageLocationCommand = new RelayCommand(OpenStorageLocation);
            ClearDataCommand = new RelayCommand(ClearData);
            OpenGithubCommand = new RelayCommand(OpenGithub);
            ReportIssueCommand = new RelayCommand(ReportIssue);
        }
        
        /// <summary>
        /// 更新主题配置
        /// </summary>
        private void UpdateTheme(string theme)
        {
            _dataService.UpdateThemeConfig(theme);
            
            // 这里需要应用主题变更到 UI
            // 由于涉及到具体的主题实现方式，这里需要考虑如何在 Avalonia 中实现主题切换
            // TODO: 实现主题切换逻辑
        }
        
        /// <summary>
        /// 更新语言配置
        /// </summary>
        private void UpdateLanguage(string language)
        {
            _dataService.UpdateLanguageConfig(language);
            _localizationService.SetLanguage(language);
            
            // TODO: 实现应用语言变更到 UI
        }
        
        /// <summary>
        /// 更新快捷键配置
        /// </summary>
        private void UpdateShortcutConfig()
        {
            var config = new ShortcutConfig
            {
                Enabled = _enableShortcut,
                Shortcut = _shortcut
            };
            
            _dataService.UpdateShortcutConfig(config);
        }
        
        /// <summary>
        /// 更新自启动配置
        /// </summary>
        private void UpdateAutoLaunch(bool enabled)
        {
            _dataService.UpdateAutoLaunchConfig(enabled);
            
            // TODO: 实现系统自启动设置
            // 需要调用特定平台的 API 来设置自启动
        }
        
        /// <summary>
        /// 打开存储位置
        /// </summary>
        private void OpenStorageLocation()
        {
            try
            {
                var userDataFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "LauncherAppAvalonia",
                    "UserData");
                
                if (System.IO.Directory.Exists(userDataFolder))
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = userDataFolder,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(startInfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening storage location: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 清空数据
        /// </summary>
        private void ClearData()
        {
            // 显示确认对话框
            // 在 Avalonia 中，我们需要使用一个独立的确认对话框
            // 这里简化处理，直接清空数据
            // 实际应用中应添加确认对话框
            _dataService.ClearAllItems();
        }
        
        /// <summary>
        /// 打开 GitHub 页面
        /// </summary>
        private void OpenGithub()
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/SolarianZ/LauncherAppAvalonia",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening GitHub: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 报告问题
        /// </summary>
        private void ReportIssue()
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/SolarianZ/LauncherAppAvalonia/issues",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reporting issue: {ex.Message}");
            }
        }
    }
}