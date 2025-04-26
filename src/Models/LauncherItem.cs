using System;

namespace LauncherAppAvalonia.Models
{
    /// <summary>
    /// 表示启动器中的一个项目
    /// </summary>
    public class LauncherItem
    {
        /// <summary>
        /// 项目的路径、URL或命令
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 项目类型
        /// </summary>
        public PathType Type { get; set; }

        /// <summary>
        /// 项目的显示名称（可选）
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 最后访问时间
        /// </summary>
        public DateTime LastAccessed { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建一个新的启动器项目
        /// </summary>
        public LauncherItem() { }

        /// <summary>
        /// 创建一个新的启动器项目
        /// </summary>
        /// <param name="path">项目的路径、URL或命令</param>
        /// <param name="type">项目类型</param>
        /// <param name="name">项目的显示名称（可选）</param>
        public LauncherItem(string path, PathType type, string? name = null)
        {
            Path = path;
            Type = type;
            Name = name;
        }
    }
}