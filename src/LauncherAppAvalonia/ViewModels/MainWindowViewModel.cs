using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LauncherAppAvalonia.Models;
using LauncherAppAvalonia.Services;

namespace LauncherAppAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public bool IsItemEditorViewVisible => ItemEditorViewModel is not null;

    [ObservableProperty]
    private ObservableCollection<LauncherItem> _items;

    [ObservableProperty]
    private string? _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LauncherItemViewModel> _filteredItems = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsItemEditorViewVisible))]
    private ItemEditorViewModel? _itemEditorViewModel;

    private readonly DataService _dataService;


    public MainWindowViewModel(DataService dataService)
    {
        _dataService = dataService;

        // 初始化数据
        List<LauncherItem> items = _dataService.GetItems();
        _items = new ObservableCollection<LauncherItem>(items);
        Items.CollectionChanged += (_, _) => UpdateFilteredItems();
        UpdateFilteredItems();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(SearchText) or nameof(Items))
        {
            UpdateFilteredItems();
        }
    }

    private void UpdateFilteredItems()
    {
        FilteredItems.Clear();
        foreach (LauncherItem item in Items)
        {
            if (MatchesSearchText(item, SearchText))
                FilteredItems.Add(new LauncherItemViewModel(item));
        }
    }

    private static bool MatchesSearchText(LauncherItem item, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return true;

        return item.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
            item.Path.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            item.Type.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}