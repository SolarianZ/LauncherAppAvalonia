using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LauncherAppAvalonia.Services;

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

            // 创建主窗口
            _mainWindow = new MainWindow();
            desktop.MainWindow = _mainWindow;

            // 初始化托盘服务
            TrayService = new TrayService(DataService, ItemHandlerService, LocalizationService);
            TrayService.CreateTrayIcon(ShowMainWindow);

            // 注册退出事件处理
            desktop.Exit += OnAppExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow != null)
        {
            _mainWindow.Show();
            _mainWindow.Activate();
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

        // 释放全局资源
        DataService = null;
        ItemHandlerService = null;
        LocalizationService = null;
        TrayService = null;
    }
}