using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.ViewModels;

public partial class MainWindowViewModel
{
    public ICommand AddItemCommand { get; private set; } = null!;
    public ICommand OpenSettingsCommand { get; private set; } = null!;
    public ICommand EditItemCommand { get; private set; } = null!;
    public ICommand RemoveItemCommand { get; private set;} = null!;
    public ICommand OpenItemCommand { get; private set; } = null!;
    public ICommand ShowItemInFolderCommand { get; private set; } = null!;
    public ICommand CopyItemPathCommand { get; private set; } = null!;


    private void InitializeCommands()
    {
        AddItemCommand = new RelayCommand<Control?>(ExecuteAddItemCommand, CanExecuteAddItemCommand);
        OpenSettingsCommand = new RelayCommand<Control?>(ExecuteOpenSettingsCommand, CanExecuteOpenSettingsCommand);
        EditItemCommand = new RelayCommand<LauncherItemViewModel?>(ExecuteEditItemCommand, CanExecuteEditItemCommand);
        RemoveItemCommand = new RelayCommand<LauncherItemViewModel?>(ExecuteRemoveItemCommand, CanExecuteRemoveItemCommand);
        OpenItemCommand = new RelayCommand<LauncherItemViewModel?>(ExecuteOpenItemCommand, CanExecuteOpenItemCommand);
        ShowItemInFolderCommand = new RelayCommand<LauncherItemViewModel?>(ExecuteShowItemInFolderCommand, CanExecuteShowItemInFolderCommand);
        CopyItemPathCommand = new RelayCommand<LauncherItemViewModel?>(ExecuteCopyItemPathCommand, CanExecuteCopyItemPathCommand);
    }


    #region AddItemCommand

    private bool CanExecuteAddItemCommand(Control? addButton) => addButton != null;

    private void ExecuteAddItemCommand(Control? addButton)
    {
        Debug.Assert(addButton != null);
        FlyoutBase.ShowAttachedFlyout(addButton);
    }

    #endregion


    #region OpenSettingsCommand

    private bool CanExecuteOpenSettingsCommand(Control? addButton) => addButton != null;

    private void ExecuteOpenSettingsCommand(Control? addButton)
    {
        Debug.Assert(addButton != null);
        FlyoutBase.ShowAttachedFlyout(addButton);
    }

    #endregion


    #region EditItemCommand

    private bool CanExecuteEditItemCommand(LauncherItemViewModel? itemVM)
    {
        return itemVM != null;
    }

    private void ExecuteEditItemCommand(LauncherItemViewModel? itemVM)
    {
        Debug.Assert(itemVM != null);
        Debug.WriteLine($"TODO Edit Item: [{itemVM.Type}] {itemVM.Path}");
    }

    #endregion


    #region RemoveItemCommand

    private bool CanExecuteRemoveItemCommand(LauncherItemViewModel? itemVM)
    {
        return itemVM != null;
    }

    private void ExecuteRemoveItemCommand(LauncherItemViewModel? itemVM)
    {
        Debug.Assert(itemVM != null);
        Debug.WriteLine($"TODO Remove Item: [{itemVM.Type}] {itemVM.Path}");
    }

    #endregion


    #region OpenItemCommand

    private bool CanExecuteOpenItemCommand(LauncherItemViewModel? itemVM)
    {
        return itemVM != null;
    }

    private void ExecuteOpenItemCommand(LauncherItemViewModel? itemVM)
    {
        Debug.Assert(itemVM != null);

        try
        {
            switch (itemVM.Type)
            {
                case LauncherItemType.File:
                    OpenFile(itemVM.Path);
                    break;
                case LauncherItemType.Folder:
                    OpenFolder(itemVM.Path);
                    break;
                case LauncherItemType.Url:
                    OpenUrl(itemVM.Path);
                    break;
                case LauncherItemType.Command:
                    OpenCommand(itemVM.Path);
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

    private void OpenFile(string filePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start(new ProcessStartInfo("open", $"\"{filePath}\"")
            {
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Process.Start(new ProcessStartInfo("xdg-open", $"\"{filePath}\"")
            {
                UseShellExecute = true
            });
        }
        else
        {
            Debug.WriteLine($"Unsupported OS for opening file: {RuntimeInformation.OSDescription}");
        }
    }

    private void OpenFolder(string folderPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folderPath}\"")
            {
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start(new ProcessStartInfo("open", $"\"{folderPath}\"")
            {
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Process.Start(new ProcessStartInfo("xdg-open", $"\"{folderPath}\"")
            {
                UseShellExecute = true
            });
        }
        else
        {
            Debug.WriteLine($"Unsupported OS for opening folder: {RuntimeInformation.OSDescription}");
        }
    }

    private void OpenUrl(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start(new ProcessStartInfo("open", url)
            {
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Process.Start(new ProcessStartInfo("xdg-open", url)
            {
                UseShellExecute = true
            });
        }
        else
        {
            Debug.WriteLine($"Unsupported OS for opening URL: {RuntimeInformation.OSDescription}");
        }
    }

    private void OpenCommand(string command)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!command.StartsWith("/C", StringComparison.OrdinalIgnoreCase) &&
                !command.StartsWith("/K", StringComparison.OrdinalIgnoreCase))
                command = $"/K {command}";

            Process.Start(new ProcessStartInfo("cmd.exe", command)
            {
                UseShellExecute = false
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Process.Start(new ProcessStartInfo("/bin/bash", $"-c \"{command}\"")
            {
                UseShellExecute = false
            });
        }
        else
        {
            Debug.WriteLine($"Unsupported OS for executing command: {RuntimeInformation.OSDescription}");
        }
    }

    #endregion


    #region ShowItemInFolderCommand

    private bool CanExecuteShowItemInFolderCommand(LauncherItemViewModel? itemVM)
    {
        if (itemVM == null)
            return false;

        switch (itemVM.Type)
        {
            case LauncherItemType.File:
                return File.Exists(itemVM.Path);
            case LauncherItemType.Folder:
                return Directory.Exists(itemVM.Path);
            default:
                return false;
        }
    }

    private void ExecuteShowItemInFolderCommand(LauncherItemViewModel? itemVM)
    {
        Debug.Assert(itemVM is { Type: LauncherItemType.File or LauncherItemType.Folder });

        try
        {
            bool isFolder = itemVM.Type == LauncherItemType.Folder;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                ShowItemInFolder_Windows(itemVM.Path, isFolder);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                ShowItemInFolder_OSX(itemVM.Path, isFolder);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                ShowItemInFolder_UNIX(itemVM.Path, isFolder);
            else
                Debug.WriteLine($"Unsupported OS for showing item in folder: {RuntimeInformation.OSDescription}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing item in folder: {ex.Message}");
        }
    }

    private void ShowItemInFolder_Windows(string path, bool isFolder)
    {
        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"")
        {
            UseShellExecute = true
        });
    }

    private void ShowItemInFolder_OSX(string path, bool isFolder)
    {
        Process.Start(new ProcessStartInfo("open", $"-R \"{path}\"")
        {
            UseShellExecute = true
        });
    }

    private void ShowItemInFolder_UNIX(string path, bool isFolder)
    {
        string command = isFolder ? "xdg-open" : "xdg-open --select";
        Process.Start(new ProcessStartInfo(command, $"\"{path}\"")
        {
            UseShellExecute = true
        });
    }

    #endregion


    #region CopyItemPathCommand

    private bool CanExecuteCopyItemPathCommand(LauncherItemViewModel? itemVM)
    {
        return itemVM != null;
    }


    private void ExecuteCopyItemPathCommand(LauncherItemViewModel? itemVM)
    {
        Debug.Assert(itemVM != null);

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