using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.ViewModels;

public class LauncherItemViewModel(LauncherItem launcherItem) : ViewModelBase
{
    public LauncherItem LauncherItem { get; } = launcherItem;
    public LauncherItemType Type => LauncherItem.Type;
    public string Path => LauncherItem.Path;
    public string? Name => string.IsNullOrEmpty(LauncherItem.Name)
        ? LauncherItem.Path
        : LauncherItem.Name;
    public string Icon => LauncherItem.Type switch
    {
        LauncherItemType.File => "📄",
        LauncherItemType.Folder => "📂",
        LauncherItemType.Url => "🌐",
        LauncherItemType.Command => "⌨",
        _ => "❓"
    };
}