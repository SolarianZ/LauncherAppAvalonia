using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace LauncherAppAvalonia;

public static class Utility
{
    #region Show Item in Folder

    public static void ShowItemInFolder(string path)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"")
                {
                    UseShellExecute = true
                });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start(new ProcessStartInfo("open", $"-R \"{path}\"")
                {
                    UseShellExecute = true
                });
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                bool isFolder = Directory.Exists(path);
                string command = isFolder ? "xdg-open" : "xdg-open --select";
                Process.Start(new ProcessStartInfo(command, $"\"{path}\"")
                {
                    UseShellExecute = true
                });
            }
            else
                Debug.WriteLine($"Unsupported OS for showing item in folder: {RuntimeInformation.OSDescription}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error showing item in folder: {ex.Message}");
        }
    }

    #endregion


    #region Open Item

    public static void OpenFile(string filePath)
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

    public static void OpenFolder(string folderPath)
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

    public static void OpenUrl(string url)
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

    public static void OpenCommand(string command)
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
}