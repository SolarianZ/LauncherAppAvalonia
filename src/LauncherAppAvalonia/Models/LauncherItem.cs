using CommunityToolkit.Mvvm.ComponentModel;

namespace LauncherAppAvalonia.Models;

public enum LauncherItemType
{
    File,
    Folder,
    Url,
    Command,
}

public partial class LauncherItem(LauncherItemType type, string path, string? name)
    : ObservableObject
{
    [ObservableProperty]
    private LauncherItemType _type = type;

    [ObservableProperty]
    private string _path = path;

    [ObservableProperty]
    private string? _name = name;
}