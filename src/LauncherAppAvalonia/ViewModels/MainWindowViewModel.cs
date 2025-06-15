using System.Collections.Generic;
using System.Collections.ObjectModel;
using LauncherAppAvalonia.Models;
using LauncherAppAvalonia.Services;

namespace LauncherAppAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<LauncherItemViewModel> FilteredItems { get; } = new();


    private readonly DataService _dataService;


    // For previewer use
    internal MainWindowViewModel() { }

    public MainWindowViewModel(DataService dataService)
    {
        _dataService = dataService;

        // 初始化数据
        List<LauncherItem> items = _dataService.GetItems();
        foreach (LauncherItem item in items)
        {
            FilteredItems.Add(new LauncherItemViewModel(item));
        }

        InitializeCommands();
    }
}