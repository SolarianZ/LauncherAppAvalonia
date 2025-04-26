using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;

namespace LauncherAppAvalonia.Services
{
    /// <summary>
    /// 本地化服务，提供多语言支持
    /// </summary>
    public class LocalizationService
    {
        private Dictionary<string, Dictionary<string, string>> _translations = new();
        private string _currentLanguage = "en-US";
        private readonly string _defaultLanguage = "en-US";
        private readonly string _localesFolder;
        private readonly string _userLocalesFolder;

        // 语言变化事件
        public event EventHandler<string>? LanguageChanged;

        public LocalizationService()
        {
            // 获取应用程序路径
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            _localesFolder = Path.Combine(appPath, "Assets", "Locales");

            // 用户自定义语言文件夹
            _userLocalesFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LauncherAppAvalonia",
                "Locales");

            // 确保目录存在
            if (!Directory.Exists(_userLocalesFolder))
            {
                Directory.CreateDirectory(_userLocalesFolder);
            }

            // 加载所有可用的语言文件
            LoadAvailableLanguages();

            // 设置初始语言为系统语言
            _currentLanguage = GetSystemLanguage();
        }

        /// <summary>
        /// 加载所有可用的语言文件
        /// </summary>
        private void LoadAvailableLanguages()
        {
            try
            {
                // 加载内置语言
                if (Directory.Exists(_localesFolder))
                {
                    foreach (var file in Directory.GetFiles(_localesFolder, "*.json"))
                    {
                        string langCode = Path.GetFileNameWithoutExtension(file);
                        LoadLanguageFile(langCode, file);
                    }
                }

                // 加载用户自定义语言
                if (Directory.Exists(_userLocalesFolder))
                {
                    foreach (var file in Directory.GetFiles(_userLocalesFolder, "*.json"))
                    {
                        string langCode = Path.GetFileNameWithoutExtension(file);
                        LoadLanguageFile(langCode, file);
                    }
                }

                // 确保至少有一个语言可用
                if (!_translations.ContainsKey(_defaultLanguage))
                {
                    _translations[_defaultLanguage] = new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading language files: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载特定语言文件
        /// </summary>
        private void LoadLanguageFile(string langCode, string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (translations != null)
                    {
                        _translations[langCode] = translations;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading language file {filePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置当前语言
        /// </summary>
        public void SetLanguage(string language)
        {
            string newLang = language;

            // 处理"system"特殊值
            if (language == "system")
            {
                newLang = GetSystemLanguage();
            }

            // 如果语言不存在，使用默认语言
            if (!_translations.ContainsKey(newLang))
            {
                newLang = _defaultLanguage;
            }

            // 更新当前语言
            _currentLanguage = newLang;

            // 更新当前线程的文化
            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(newLang);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting culture: {ex.Message}");
            }

            // 触发语言变更事件
            LanguageChanged?.Invoke(this, newLang);
        }

        /// <summary>
        /// 获取系统语言
        /// </summary>
        public string GetSystemLanguage()
        {
            try
            {
                // 获取系统语言代码
                string systemLang = CultureInfo.CurrentUICulture.Name;
                
                // 如果系统语言不在我们的翻译列表中，回退到语言的基本形式（如zh-CN --> zh）
                if (!_translations.ContainsKey(systemLang) && systemLang.Contains('-'))
                {
                    string baseLang = systemLang.Split('-')[0];
                    if (_translations.Keys.Any(k => k.StartsWith(baseLang + "-")))
                    {
                        return _translations.Keys.First(k => k.StartsWith(baseLang + "-"));
                    }
                }

                // 如果翻译存在，返回系统语言
                if (_translations.ContainsKey(systemLang))
                {
                    return systemLang;
                }

                // 否则返回默认语言
                return _defaultLanguage;
            }
            catch
            {
                return _defaultLanguage;
            }
        }

        /// <summary>
        /// 获取翻译文本
        /// </summary>
        public string Translate(string key, object? parameters = null)
        {
            try
            {
                if (_translations.ContainsKey(_currentLanguage) && _translations[_currentLanguage].ContainsKey(key))
                {
                    string text = _translations[_currentLanguage][key];

                    // 处理参数替换（简单版本）
                    if (parameters != null)
                    {
                        foreach (var prop in parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            string placeholder = $"{{{prop.Name}}}";
                            string? value = prop.GetValue(parameters)?.ToString();
                            if (value != null)
                            {
                                text = text.Replace(placeholder, value);
                            }
                        }
                    }

                    return text;
                }

                // 如果在当前语言中找不到，尝试在默认语言中查找
                if (_currentLanguage != _defaultLanguage && 
                    _translations.ContainsKey(_defaultLanguage) && 
                    _translations[_defaultLanguage].ContainsKey(key))
                {
                    return _translations[_defaultLanguage][key];
                }

                // 如果翻译不存在，返回键本身
                return key;
            }
            catch
            {
                return key;
            }
        }

        /// <summary>
        /// 获取当前语言
        /// </summary>
        public string GetCurrentLanguage() => _currentLanguage;

        /// <summary>
        /// 获取所有可用语言
        /// </summary>
        public List<string> GetAvailableLanguages() => _translations.Keys.ToList();

        /// <summary>
        /// 获取语言名称
        /// </summary>
        public string GetLanguageName(string langCode)
        {
            try
            {
                var culture = new CultureInfo(langCode);
                return culture.NativeName;
            }
            catch
            {
                return langCode;
            }
        }
    }
}