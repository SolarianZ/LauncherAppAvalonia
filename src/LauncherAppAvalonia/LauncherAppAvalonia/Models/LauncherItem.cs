namespace LauncherAppAvalonia.Models;

public enum LauncherItemType
{
    File,
    Folder,
    Url,
    Command,
}

public class LauncherItem(LauncherItemType type, string path, string? name)
{
    public LauncherItemType Type { get; set; } = type;
    public string Path { get; set; } = path;
    public string? Name { get; set; } = name;
}