using System;

namespace LauncherAppAvalonia.Models
{
    /// <summary>
    /// 应用程序配置类
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// 主题设置："light", "dark", "system"
        /// </summary>
        public string Theme { get; set; } = "system";

        /// <summary>
        /// 语言设置："en-US", "zh-CN", "system"
        /// </summary>
        public string Language { get; set; } = "system";

        /// <summary>
        /// 快捷键配置
        /// </summary>
        public ShortcutConfig Shortcut { get; set; } = new ShortcutConfig();

        /// <summary>
        /// 启动时最小化到托盘
        /// </summary>
        public bool MinimizeToTrayOnStart { get; set; } = false;

        /// <summary>
        /// 关闭窗口时最小化到托盘
        /// </summary>
        public bool MinimizeToTrayOnClose { get; set; } = true;

        /// <summary>
        /// 启动时检查更新
        /// </summary>
        public bool CheckUpdateOnStart { get; set; } = true;
    }

    /// <summary>
    /// 快捷键配置类
    /// </summary>
    public class ShortcutConfig
    {
        /// <summary>
        /// 是否启用全局快捷键
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 快捷键组合，例如 "Alt+Shift+Q"
        /// </summary>
        public string Shortcut { get; set; } = "Alt+Shift+Q";
    }

    /// <summary>
    /// 快捷键配置变更事件参数
    /// </summary>
    public class ShortcutConfigArgs : EventArgs
    {
        /// <summary>
        /// 更新后的快捷键配置
        /// </summary>
        public ShortcutConfig Config { get; }

        /// <summary>
        /// 创建快捷键配置变更事件参数
        /// </summary>
        public ShortcutConfigArgs(ShortcutConfig config)
        {
            Config = config;
        }
    }
}