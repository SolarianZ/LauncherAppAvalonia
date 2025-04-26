namespace LauncherAppAvalonia.Models
{
    /// <summary>
    /// 路径类型枚举，用于标识不同类型的项目
    /// </summary>
    public enum PathType
    {
        /// <summary>
        /// 文件类型
        /// </summary>
        File,
        
        /// <summary>
        /// 文件夹类型
        /// </summary>
        Folder,
        
        /// <summary>
        /// 网址类型
        /// </summary>
        Url,
        
        /// <summary>
        /// 命令类型
        /// </summary>
        Command
    }
}