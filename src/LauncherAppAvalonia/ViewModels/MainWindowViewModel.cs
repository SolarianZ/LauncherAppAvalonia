using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LauncherAppAvalonia.Models;
using LauncherAppAvalonia.Services;

namespace LauncherAppAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ICommand OpenSettingsCommand { get; } = new OpenSettingsCommand();
    public ICommand AddItemCommand { get; } = new AddItemCommand();

    public ICommand OpenItemCommand { get; } = new OpenItemCommand();
    public ObservableCollection<LauncherItemViewModel> FilteredItems { get; } = new();


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