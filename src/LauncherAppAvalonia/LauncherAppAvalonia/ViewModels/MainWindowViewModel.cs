using System.Collections.ObjectModel;
using System.Windows.Input;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ICommand OpenSettingsCommand { get; } = new OpenSettingsCommand();
    public ICommand AddItemCommand { get; } = new AddItemCommand();

    public ICommand OpenItemCommand { get; } = new OpenItemCommand();
    public ObservableCollection<LauncherItemViewModel> FilteredItems { get; } = new();

    public MainWindowViewModel()
    {
        // 临时测试数据
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.File, "D:\\.GamingRoot", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Folder, "D:\\Projects", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Url, "https:www.baidu.com", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Command, "echo Hello", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.File, "D:\\.GamingRoot", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Folder, "D:\\Projects", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Url, "https:www.baidu.com", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Command, "echo Hello", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.File, "D:\\.GamingRoot", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Folder, "D:\\Projects", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Url, "https:www.baidu.com", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Command, "echo Hello", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.File, "D:\\.GamingRoot", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Folder, "D:\\Projects", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Url, "https:www.baidu.com", null)));
        FilteredItems.Add(new LauncherItemViewModel(new LauncherItem(LauncherItemType.Command, "echo Hello", null)));
    }
}