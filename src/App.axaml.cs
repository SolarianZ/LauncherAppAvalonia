using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LauncherAppAvalonia.Services;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia;

public partial class App : Application
{
    // 保持对全局服务的引用，以便在应用程序的整个生命周期内使用
    public static DataService? DataService { get; private set; }
    public static ItemHandlerService? ItemHandlerService { get; private set; }
    public static LocalizationService? LocalizationService { get; private set; }
    public static TrayService? TrayService { get; private set; }

    // 主窗口引用
    private MainWindow? _mainWindow;

    // 全局快捷键管理器
    private GlobalHotkeyManager? _hotkeyManager;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 初始化全局服务
            DataService = new DataService();
            ItemHandlerService = new ItemHandlerService();
            LocalizationService = new LocalizationService();

            // 加载应用配置
            DataService.LoadItems();
            DataService.LoadConfig();

            // 设置正确的语言
            var config = DataService.GetConfig();
            LocalizationService.SetLanguage(config.Language);

            // 注册语言变更事件监听
            LocalizationService.LanguageChanged += OnLanguageChanged;

            // 创建主窗口
            _mainWindow = new MainWindow();
            desktop.MainWindow = _mainWindow;

            // 初始化Toast服务
            ToastService.Initialize(_mainWindow);

            // 设置主题
            UpdateTheme(config.Theme);

            // 注册主题配置变更事件
            DataService.ThemeChanged += OnThemeChanged;

            // 初始化托盘服务
            TrayService = new TrayService(DataService, ItemHandlerService, LocalizationService);
            TrayService.CreateTrayIcon(ShowMainWindow);

            // 注册全局快捷键
            RegisterGlobalHotkey(config.Shortcut);

            // 注册快捷键配置变更事件
            DataService.ShortcutConfigChanged += OnShortcutConfigChanged;

            // 注册退出事件处理
            desktop.Exit += OnAppExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// 处理语言变更事件
    /// </summary>
    private void OnLanguageChanged(object? sender, string language)
    {
        // 语言已经在LocalizationService内部设置，这里可以触发一些额外的UI更新
        Console.WriteLine($"Language changed to: {language}");

        // 如果需要，可以在这里重新加载特定的UI元素或者通知窗口进行语言刷新
    }

    /// <summary>
    /// 处理主题变更事件
    /// </summary>
    private void OnThemeChanged(object? sender, string theme)
    {
        UpdateTheme(theme);
    }

    /// <summary>
    /// 处理快捷键配置变更事件
    /// </summary>
    private void OnShortcutConfigChanged(object? sender, ShortcutConfigArgs e)
    {
        // 重新注册快捷键
        RegisterGlobalHotkey(e.Config);
    }

    /// <summary>
    /// 注册全局快捷键
    /// </summary>
    private void RegisterGlobalHotkey(ShortcutConfig config)
    {
        // 释放旧的快捷键管理器
        _hotkeyManager?.Dispose();

        if (config.Enabled)
        {
            try
            {
                _hotkeyManager = new GlobalHotkeyManager();
                _hotkeyManager.Register(config.Shortcut, ShowMainWindow);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering global hotkey: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 更新应用主题
    /// </summary>
    private void UpdateTheme(string theme)
    {
        if (theme == "light")
        {
            RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
        }
        else if (theme == "dark")
        {
            RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
        }
        else
        {
            // 默认跟随系统
            RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Default;
        }

        Console.WriteLine($"Theme changed to: {theme}");
    }

    /// <summary>
    /// 显示主窗口
    /// </summary>
    private void ShowMainWindow()
    {
        if (_mainWindow != null)
        {
            _mainWindow.Show();
            _mainWindow.Activate();

            // 如果是从托盘图标或全局快捷键唤起，确保窗口在最前面
            if (_mainWindow.WindowState == Avalonia.Controls.WindowState.Minimized)
            {
                _mainWindow.WindowState = Avalonia.Controls.WindowState.Normal;
            }
        }
    }

    private void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        // 执行应用程序退出前的清理工作
        // 保存所有配置等
        DataService?.SaveConfig();
        DataService?.SaveItems();

        // 释放托盘资源
        TrayService?.Dispose();

        // 释放Toast资源
        ToastService.Dispose();

        // 释放快捷键资源
        _hotkeyManager?.Dispose();

        // 释放全局资源
        DataService = null;
        ItemHandlerService = null;
        LocalizationService = null;
        TrayService = null;
    }
}

/// <summary>
/// 全局快捷键管理器类
/// </summary>
public class GlobalHotkeyManager : IDisposable
{
    #region Windows相关API

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
    private const uint VK_Q = 0x51; // Q键的虚拟键码

    #endregion

    private bool _isDisposed = false;
    private IntPtr _windowHandle;
    private int _hotkeyId = 1;
    private Action? _callback;

    public GlobalHotkeyManager()
    {
        _windowHandle = NativeInterop.CreateMessageWindow(OnHotkeyPressed);
    }

    /// <summary>
    /// 注册全局快捷键
    /// </summary>
    public void Register(string shortcutString, Action callback)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(GlobalHotkeyManager));

        _callback = callback;

        // 解析快捷键字符串
        var (modifiers, vk) = ParseShortcut(shortcutString);

        // 注册热键
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            RegisterWindowsHotkey(modifiers, vk);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            RegisterMacHotkey(modifiers, vk);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            RegisterLinuxHotkey(modifiers, vk);
        }
    }

    /// <summary>
    /// 解析快捷键字符串
    /// </summary>
    private (uint modifiers, uint vk) ParseShortcut(string shortcut)
    {
        // 默认Alt+Shift+Q
        uint modifiers = 0;
        uint vk = VK_Q;

        var parts = shortcut.Split('+');
        foreach (var part in parts)
        {
            var key = part.Trim().ToLower();

            // 解析修饰键
            if (key == "alt")
            {
                modifiers |= MOD_ALT;
            }
            else if (key == "ctrl" || key == "control")
            {
                modifiers |= MOD_CONTROL;
            }
            else if (key == "shift")
            {
                modifiers |= MOD_SHIFT;
            }
            else if (key == "win" || key == "super" || key == "meta")
            {
                modifiers |= MOD_WIN;
            }
            // 解析主键
            else if (key.Length == 1 && key[0] >= 'a' && key[0] <= 'z')
            {
                vk = (uint)(key.ToUpper()[0]);
            }
        }

        // 确保至少有一个修饰键
        if (modifiers == 0)
        {
            modifiers = MOD_ALT | MOD_SHIFT;
        }

        return (modifiers, vk);
    }

    /// <summary>
    /// 注册Windows平台的热键
    /// </summary>
    private void RegisterWindowsHotkey(uint modifiers, uint vk)
    {
        // 注销已有的热键
        UnregisterHotKey(_windowHandle, _hotkeyId);

        // 注册新的热键
        bool success = RegisterHotKey(_windowHandle, _hotkeyId, modifiers, vk);
        if (!success)
        {
            Console.WriteLine("Failed to register Windows global hotkey");
        }
    }

    /// <summary>
    /// 注册Mac平台的热键
    /// </summary>
    private void RegisterMacHotkey(uint modifiers, uint vk)
    {
        // 注意：Mac平台需要单独的实现，这里只是一个占位符
        Console.WriteLine("Mac global hotkey registration requires platform-specific implementation");
    }

    /// <summary>
    /// 注册Linux平台的热键
    /// </summary>
    private void RegisterLinuxHotkey(uint modifiers, uint vk)
    {
        // 注意：Linux平台需要单独的实现，这里只是一个占位符
        Console.WriteLine("Linux global hotkey registration requires platform-specific implementation");
    }

    /// <summary>
    /// 处理热键按下事件
    /// </summary>
    private void OnHotkeyPressed()
    {
        _callback?.Invoke();
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UnregisterHotKey(_windowHandle, _hotkeyId);
            }

            NativeInterop.DestroyMessageWindow(_windowHandle);
            _isDisposed = true;
        }
    }
}

/// <summary>
/// 本地互操作API封装
/// </summary>
public static class NativeInterop
{
    #region Windows消息窗口API

    [DllImport("user32.dll")]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu,
        IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public System.Drawing.Point pt;
    }

    private const int WM_HOTKEY = 0x0312;

    #endregion

    /// <summary>
    /// 创建用于接收消息的隐藏窗口
    /// </summary>
    public static IntPtr CreateMessageWindow(Action callback)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // 创建隐藏的消息窗口
            IntPtr hWnd = CreateWindowEx(
                0, "STATIC", "LauncherAppHotkeyWindow", 0,
                0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            // 启动消息循环
            Task.Run(() => WindowsMessageLoop(hWnd, callback));

            return hWnd;
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Windows消息循环
    /// </summary>
    private static void WindowsMessageLoop(IntPtr hWnd, Action callback)
    {
        while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
        {
            if (msg.message == WM_HOTKEY)
            {
                callback();
            }

            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }

    /// <summary>
    /// 销毁消息窗口
    /// </summary>
    public static void DestroyMessageWindow(IntPtr hWnd)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && hWnd != IntPtr.Zero)
        {
            DestroyWindow(hWnd);
        }
    }
}