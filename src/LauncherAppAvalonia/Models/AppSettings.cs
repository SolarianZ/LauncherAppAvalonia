using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LauncherAppAvalonia.Models;

public partial class AppSettings
    : ObservableObject
{
    public static IReadOnlyList<string> Themes { get; } = ["System", "Light", "Dark"];
    public static IReadOnlyList<string> Languages { get; } = ["System", "zh-CN", "en-US"];

    [ObservableProperty]
    private string _theme = Themes[0];

    [ObservableProperty]
    private string _language = Languages[0];

    [ObservableProperty]
    private string? _hotkey = "Alt+Shift+Q";


    public AppSettings() { }

    public AppSettings(AppSettings source) => Clone(source);

    public void Reset()
    {
        Theme = Themes[0];
        Language = Languages[0];
        Hotkey = "Alt+Shift+Q";
    }

    public void Clone(AppSettings source)
    {
        Theme = source.Theme;
        Language = source.Language;
        Hotkey = source.Hotkey;
    }
}