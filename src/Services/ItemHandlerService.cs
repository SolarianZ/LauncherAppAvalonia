using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using LauncherAppAvalonia.Models;

namespace LauncherAppAvalonia.Services
{
    /// <summary>
    /// 项目处理服务，负责处理项目的打开、执行等操作
    /// </summary>
    public class ItemHandlerService
    {
        /// <summary>
        /// 处理项目动作（打开文件、文件夹、URL或执行命令）
        /// </summary>
        public void HandleItemAction(LauncherItem item)
        {
            try
            {
                switch (item.Type)
                {
                    case PathType.File:
                        OpenFile(item.Path);
                        break;
                    case PathType.Folder:
                        OpenFolder(item.Path);
                        break;
                    case PathType.Url:
                        OpenUrl(item.Path);
                        break;
                    case PathType.Command:
                        ExecuteCommand(item.Path);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling item action: {ex.Message}");
            }
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        private void OpenFile(string path)
        {
            if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        private void OpenFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
        }

        /// <summary>
        /// 打开URL
        /// </summary>
        private void OpenUrl(string url)
        {
            // 检查URL格式，添加https://前缀如果需要
            if (!url.StartsWith("http://") && !url.StartsWith("https://") && 
                !url.StartsWith("ftp://") && !Regex.IsMatch(url, @"^\w+:\/\/"))
            {
                // 检查是否是标准域名格式
                if (Regex.IsMatch(url, @"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}(\/.*)?$"))
                {
                    url = "https://" + url;
                }
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        private void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            // 创建临时工作目录
            string workDir = Path.Combine(Path.GetTempPath(), "launcher-app-temp");
            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ExecuteCommandForWindows(command, workDir);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ExecuteCommandForMac(command, workDir);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ExecuteCommandForLinux(command, workDir);
            }
        }

        /// <summary>
        /// Windows平台执行命令
        /// </summary>
        private void ExecuteCommandForWindows(string command, string workDir)
        {
            // 使用CMD执行命令
            var processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/K \"{command}\"",
                WorkingDirectory = workDir,
                UseShellExecute = true
            };
            Process.Start(processInfo);
        }

        /// <summary>
        /// Mac平台执行命令
        /// </summary>
        private void ExecuteCommandForMac(string command, string workDir)
        {
            // 在Mac上，使用Terminal.app执行命令
            var escapedCommand = command.Replace("\"", "\\\"");
            var script = $"tell application \"Terminal\"\n" +
                         $"    do script \"cd \\\"{workDir}\\\" && {escapedCommand}\"\n" +
                         $"    activate\n" +
                         $"end tell";

            var processInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{script}\"",
                UseShellExecute = true
            };
            Process.Start(processInfo);
        }

        /// <summary>
        /// Linux平台执行命令
        /// </summary>
        private void ExecuteCommandForLinux(string command, string workDir)
        {
            // Linux有多种不同的终端模拟器，需要尝试多种终端
            var escapedCommand = command.Replace("\"", "\\\"");
            var escapedWorkDir = workDir.Replace("\"", "\\\"");
            
            // 尝试常见的几种终端
            string[] terminals = {
                $"gnome-terminal -- bash -c \"cd \\\"{escapedWorkDir}\\\" && {escapedCommand}; exec bash\"",
                $"konsole --noclose -e bash -c \"cd \\\"{escapedWorkDir}\\\" && {escapedCommand}\"",
                $"xterm -hold -e bash -c \"cd \\\"{escapedWorkDir}\\\" && {escapedCommand}\"",
                $"x-terminal-emulator -e bash -c \"cd \\\"{escapedWorkDir}\\\" && {escapedCommand}; exec bash\""
            };

            foreach (var terminalCommand in terminals)
            {
                try
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "bash",
                        Arguments = $"-c \"{terminalCommand}\"",
                        UseShellExecute = true
                    });

                    if (process != null)
                        return; // 成功启动一个终端
                }
                catch
                {
                    continue; // 尝试下一个终端
                }
            }
        }

        /// <summary>
        /// 在文件夹中显示项目
        /// </summary>
        public void ShowItemInFolder(string path)
        {
            if (File.Exists(path))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{path}\"",
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "open",
                        Arguments = $"-R \"{path}\"",
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // 获取文件所在目录
                    string parentDir = Path.GetDirectoryName(path) ?? string.Empty;
                    if (!string.IsNullOrEmpty(parentDir))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "xdg-open",
                            Arguments = $"\"{parentDir}\"",
                            UseShellExecute = true
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 根据路径判断项目类型
        /// </summary>
        public PathType GetItemType(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return PathType.Command; // 默认为命令类型

            // 判断是否是文件或文件夹
            if (File.Exists(path))
                return PathType.File;

            if (Directory.Exists(path))
                return PathType.Folder;

            // 判断是否是URL
            bool isUrl = Regex.IsMatch(path, @"^(\w+:\/\/|www\.)") || 
                         Regex.IsMatch(path, @"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}(\/.*)?$");

            if (isUrl)
                return PathType.Url;

            // 默认为命令
            return PathType.Command;
        }
    }
}