using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LauncherAppAvalonia.Models;
using LauncherAppAvalonia.Services;

namespace LauncherAppAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    public ObservableCollection<LauncherItemViewModel> FilteredItems { get; } = new();

    public bool IsItemEditorViewVisible => ItemEditorViewModel is not null;

    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsItemEditorViewVisible))]
    private ItemEditorViewModel? _itemEditorViewModel;

    private readonly DataService _dataService;


    public MainWindowViewModel(DataService dataService)
    {
        _dataService = dataService;

        // 初始化数据
        List<LauncherItem> items = _dataService.GetItems();
        foreach (LauncherItem item in items)
        {
            FilteredItems.Add(new LauncherItemViewModel(item));
        }
    }
}