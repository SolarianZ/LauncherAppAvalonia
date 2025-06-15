using System;

namespace LauncherAppAvalonia.Models;

public class AppSettings
{
    /// <summary>
    /// 主题（System/Light/Dark）
    /// </summary>
    public string Theme { get; set; } = null!;

    /// <summary>
    /// 语言（System/zh-CN/en-US）
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 快捷键
    /// </summary>
    public string? Hotkey { get; set; }


    public AppSettings() => Reset();

    public AppSettings(AppSettings source) => Clone(source);

    public void Reset()
    {
        Theme = "System";
        Language = "System";
        Hotkey = "Alt+Shift+Q";
    }

    public void Clone(AppSettings source)
    {
        Theme = source.Theme;
        Language = source.Language;
        Hotkey = source.Hotkey;
    }
}