using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.ViewModels;

public partial class ItemEditorViewModel : ViewModelBase
{
    public LauncherItemType[] ItemTypes { get; } = Enum.GetValues<LauncherItemType>();

    [ObservableProperty]
    private string? _path = string.Empty;

    [ObservableProperty]
    private string? _name = string.Empty;

    [ObservableProperty]
    private LauncherItemType _type = LauncherItemType.Command;


    public void SetItem(LauncherItem? item)
    {
        Path = item?.Path;
        Name = item?.Name;
        Type = item?.Type ?? LauncherItemType.Command;
    }


    #region Commands

    [RelayCommand]
    private async Task SelectFileAsync()
    {
        // 调用内置文件选择对话框
        TopLevel? topLevel = GetTopLevel();
        if (topLevel is null)
            return;

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Select File",
                AllowMultiple = false
            });
        if (files.Count > 0)
            Path = files[0].Path.LocalPath;
    }

    [RelayCommand]
    private async Task SelectFolderAsync()
    {
        // 调用内置文件夹选择对话框
        TopLevel? topLevel = GetTopLevel();
        if (topLevel is null)
            return;

        IReadOnlyList<IStorageFolder> folders = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select Folder",
                AllowMultiple = false
            });
        if (folders.Count > 0)
            Path = folders[0].Path.LocalPath;
    }

    [RelayCommand]
    private void SetInvisible()
    {
        // TODO 隐藏界面
    }

    [RelayCommand]
    private async Task SaveAndSetInvisibleAsync()
    {
        // 隐藏界面
        SetInvisible();

        // TODO 保存
    }

    #endregion


    private static TopLevel? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        return null;
    }
}