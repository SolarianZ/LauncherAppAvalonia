using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.ViewModels;

public partial class MainWindowViewModel
{
    #region AddItem & EditItem

    [RelayCommand]
    private void AddItem(IBrush? background)
    {
        Debug.Assert(IsItemEditorViewVisible == false);

        LauncherItem newItem = new LauncherItem(LauncherItemType.Command, string.Empty, null);
        ItemEditorViewModel = new ItemEditorViewModel(newItem, CloseItemEditorView, SaveItemAndCloseItemEditorView)
        {
            ViewBackground = background,
        };
    }

    [RelayCommand]
    private void EditItem(LauncherItemViewModel itemVM)
    {
        Debug.Assert(itemVM != null);
        Debug.Assert(IsItemEditorViewVisible == false);

        LauncherItem item = itemVM.LauncherItem;
        ItemEditorViewModel = new ItemEditorViewModel(item, CloseItemEditorView, SaveItemAndCloseItemEditorView);
    }

    private void CloseItemEditorView()
    {
        Debug.Assert(IsItemEditorViewVisible);

        ItemEditorViewModel = null;
    }

    private void SaveItemAndCloseItemEditorView(LauncherItem item)
    {
        Debug.Assert(IsItemEditorViewVisible);

        if (!Items.Contains(item))
            Items.Add(item);
        _dataService.SetItems(Items);

        CloseItemEditorView();
    }

    #endregion


    #region OpenSettings

    [RelayCommand]
    private void OpenAppSettingsView(IBrush? background)
    {
        Debug.Assert(IsAppSettingsViewVisible == false);

        AppSettings appSettings = _dataService.GetSettings();
        AppSettingsViewModel = new AppSettingsViewModel(appSettings, CloseAppSettingsView, SaveAppSettings)
        {
            ViewBackground = background,
        };
    }

    private void CloseAppSettingsView()
    {
        Debug.Assert(IsAppSettingsViewVisible);

        AppSettingsViewModel = null;
    }

    private void SaveAppSettings(AppSettings appSettings)
    {
        Debug.Assert(IsAppSettingsViewVisible);

        _dataService.SetSettings(appSettings);
    }

    #endregion


    #region RemoveItem

    [RelayCommand]
    private void RemoveItem(LauncherItemViewModel itemVM)
    {
        LauncherItem item = itemVM.LauncherItem;
        Items.Remove(item);
        _dataService.SetItems(Items);
    }

    #endregion


    #region OpenItem

    [RelayCommand]
    private void OpenItem(LauncherItemViewModel itemVM)
    {
        Debug.Assert(itemVM != null);

        try
        {
            switch (itemVM.Type)
            {
                case LauncherItemType.File:
                    Utility.OpenFile(itemVM.Path);
                    break;
                case LauncherItemType.Folder:
                    Utility.OpenFolder(itemVM.Path);
                    break;
                case LauncherItemType.Url:
                    Utility.OpenUrl(itemVM.Path);
                    break;
                case LauncherItemType.Command:
                    Utility.OpenCommand(itemVM.Path);
                    break;
                default:
                    Debug.WriteLine("Unsupported item type for opening: " + itemVM.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening item: {ex.Message}");
        }
    }

    #endregion


    #region ShowItemInFolder

    private bool CanShowItemInFolder(LauncherItemViewModel? itemVM)
    {
        if (itemVM == null)
            return false;

        return itemVM.Type switch
        {
            LauncherItemType.File => File.Exists(itemVM.Path),
            LauncherItemType.Folder => Directory.Exists(itemVM.Path),
            _ => false
        };
    }

    [RelayCommand(CanExecute = nameof(CanShowItemInFolder))]
    private void ShowItemInFolder(LauncherItemViewModel itemVM)
    {
        Debug.Assert(itemVM is { Type: LauncherItemType.File or LauncherItemType.Folder });

        Utility.ShowItemInFolder(itemVM.Path);
    }

    #endregion


    #region CopyItemPath

    [RelayCommand]
    private void CopyItemPath(LauncherItemViewModel itemVM)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                IClipboard? clipboard = TopLevel.GetTopLevel(desktop.MainWindow)?.Clipboard;
                if (clipboard != null)
                {
                    clipboard.SetTextAsync(itemVM.Path);
                    return;
                }
            }

            Debug.WriteLine("Clipboard service is not available.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error copying item path: {ex.Message}");
        }
    }

    #endregion
}