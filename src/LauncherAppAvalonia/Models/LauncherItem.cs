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
    public LauncherItemType _type = type;
    [ObservableProperty]
    public string _path = path;
    [ObservableProperty]
    public string? _name = name;
}