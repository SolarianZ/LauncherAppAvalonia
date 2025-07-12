using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.ViewModels;

public partial class ItemEditorViewModel : ViewModelBase
{
    public static LauncherItemType[] ItemTypes { get; } = Enum.GetValues<LauncherItemType>();

    [ObservableProperty]
    private LauncherItem _launcherItem;

    [ObservableProperty]
    private IBrush? _viewBackground;

    // 匹配如http://, https://, ftp://, app://, myapp://等协议格式
    private readonly Regex _protocolRegex = new(@"^[a-z][a-z0-9+.-]*:\/\/", RegexOptions.IgnoreCase);
    // 匹配标准域名格式 (包括www开头和不带www的域名)
    private readonly Regex _domainRegex = new(@"^([a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,})(:[0-9]{1,5})?(\/.*)?$", RegexOptions.IgnoreCase);

    private readonly Action _onCancel;
    private readonly Action<LauncherItem> _onSave;


    public ItemEditorViewModel(LauncherItem item, Action onCancel, Action<LauncherItem> onSave)
    {
        LauncherItem = item;

        _onCancel = onCancel;
        _onSave = onSave;

        LauncherItem.PropertyChanged += OnLauncherItemPropertyChanged;
    }

    private void OnLauncherItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Path))
            DetectItemTypeByPath(LauncherItem.Path);
    }

    private void DetectItemTypeByPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            LauncherItem.Type = LauncherItemType.Command;
            return;
        }

        if (File.Exists(path))
        {
            // Windows特殊逻辑：将BAT视为Command
            if (OperatingSystem.IsWindows())
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();
                if (extension == ".bat")
                {
                    LauncherItem.Type = LauncherItemType.Command;
                    return;
                }
            }

            LauncherItem.Type = LauncherItemType.File;
            return;
        }

        if (Directory.Exists(path))
        {
            LauncherItem.Type = LauncherItemType.Folder;
            return;
        }

        // 判断是否是标准协议URL和Deep Link
        if (_protocolRegex.IsMatch(path) || _domainRegex.IsMatch(path))
        {
            LauncherItem.Type = LauncherItemType.Url;
            return;
        }

        // 默认视为 Command
        LauncherItem.Type = LauncherItemType.Command;
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
            LauncherItem.Path = files[0].Path.LocalPath;
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
            LauncherItem.Path = folders[0].Path.LocalPath;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        _onCancel.Invoke();
    }

    [RelayCommand]
    private void SaveEdit()
    {
        _onSave.Invoke(LauncherItem);
    }

    #endregion


    private static TopLevel? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        return null;
    }
}