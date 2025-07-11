using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
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

        ItemEditorViewModel = new ItemEditorViewModel(null, CloseItemEditorView, SaveItemAndCloseItemEditorView)
        {
            ViewBackground = background ?? Brushes.Linen,
        };
    }

    [RelayCommand]
    private void EditItem(LauncherItemViewModel? itemVM)
    {
        Debug.Assert(itemVM != null);
        Debug.Assert(IsItemEditorViewVisible == false);

        LauncherItem item = itemVM.LauncherItem;
        ItemEditorViewModel = new ItemEditorViewModel(item, CloseItemEditorView, SaveItemAndCloseItemEditorView);
    }

    private void CloseItemEditorView()
    {
        Debug.Assert(ItemEditorViewModel != null);
        ItemEditorViewModel = null;
    }

    private void SaveItemAndCloseItemEditorView()
    {
        Debug.Assert(ItemEditorViewModel != null);

        // TODO SaveItem

        CloseItemEditorView();
    }

    #endregion


    #region OpenSettings

    [RelayCommand]
    private void OpenSettings(Control? addButton)
    {
        Debug.Assert(addButton != null);
        FlyoutBase.ShowAttachedFlyout(addButton);
    }

    #endregion


    #region RemoveItem

    [RelayCommand]
    private void RemoveItem(LauncherItemViewModel? itemVM)
    {
        Debug.Assert(itemVM != null);
        Debug.WriteLine($"TODO Remove Item: [{itemVM.Type}] {itemVM.Path}");
    }

    #endregion


    #region OpenItem

    [RelayCommand]
    private void OpenItem(LauncherItemViewModel? itemVM)
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
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo("open", $"\"{filePath}\"")
            {
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsLinux() ||
                 OperatingSystem.IsFreeBSD())
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
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folderPath}\"")
            {
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo("open", $"\"{folderPath}\"")
            {
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsLinux() ||
                 OperatingSystem.IsFreeBSD())
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
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo("open", url)
            {
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsLinux() ||
                 OperatingSystem.IsFreeBSD())
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
        if (OperatingSystem.IsWindows())
        {
            if (!command.StartsWith("/C", StringComparison.OrdinalIgnoreCase) &&
                !command.StartsWith("/K", StringComparison.OrdinalIgnoreCase))
                command = $"/K {command}";

            Process.Start(new ProcessStartInfo("cmd.exe", command)
            {
                UseShellExecute = false
            });
        }
        else if (OperatingSystem.IsMacOS() ||
                 OperatingSystem.IsLinux() ||
                 OperatingSystem.IsFreeBSD())
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


    #region ShowItemInFolder

    private bool CanShowItemInFolder(LauncherItemViewModel? itemVM)
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

    [RelayCommand(CanExecute = nameof(CanShowItemInFolder))]
    private void ShowItemInFolder(LauncherItemViewModel? itemVM)
    {
        Debug.Assert(itemVM is { Type: LauncherItemType.File or LauncherItemType.Folder });

        try
        {
            bool isFolder = itemVM.Type == LauncherItemType.Folder;
            if (OperatingSystem.IsWindows())
                ShowItemInFolder_Windows(itemVM.Path, isFolder);
            else if (OperatingSystem.IsMacOS())
                ShowItemInFolder_OSX(itemVM.Path, isFolder);
            else if (OperatingSystem.IsLinux() ||
                     OperatingSystem.IsFreeBSD())
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


    #region CopyItemPath

    [RelayCommand]
    private void CopyItemPath(LauncherItemViewModel? itemVM)
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