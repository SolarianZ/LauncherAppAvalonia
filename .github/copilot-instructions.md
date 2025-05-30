# GitHub Copilot 指导文件

## 项目概述

Launcher App 是一个基于 Avalonia UI 的启动器应用程序，用于管理用户常用的文件、文件夹、URL 和指令。
该应用程序包含项目列表界面（主界面）、项目编辑界面、项目右键菜单、设置界面、托盘图标、托盘右键菜单等。
支持多平台（Windows, macOS, Linux）、多语言（中文、英文）和主题切换（浅色、深色）。

## 主要功能

1. **项目管理**
   - 添加、编辑和移除项目（文件、文件夹、URL、命令）
   - 拖放添加文件/文件夹
   - 项目排序与搜索
   - 右键菜单操作（打开、复制、在文件夹中显示等）

2. **用户界面**
   - 主窗口（项目列表）
   - 项目编辑窗口
   - 设置窗口
   - 系统托盘集成
   - 键盘导航与快捷键
   - 统一的Toast提示组件

3. **系统集成**
   - 全局快捷键（Alt+Shift+Q 呼出应用）
   - 系统托盘菜单（显示最近8个项目）
   - 跨平台终端命令执行
   - 系统文件操作（在文件夹中显示、复制文件）

4. **设置与配置**
   - 主题设置（浅色/深色/跟随系统）
   - 语言设置（中文/英文/跟随系统）
   - 数据管理（清空数据、查看数据存储位置）

## 项目结构

```
LauncherAppAvalonia/src/
├── Program.cs                 # 应用入口
└── TODO 其他待补充...
```

## 编码规范

此项目是 Avalonia UI 技术的示例项目，需要遵守以下规范，以便入门者阅读学习。

1. 注重代码质量和可读性，使用有意义的变量和函数名称。
2. 添加详细的注释，尤其是处理平台特定代码时。
3. 分离关注点，将不同功能模块化处理。
4. 使用异步编程处理IO操作，避免阻塞主线程。
5. 避免代码重复，将常用功能提取为模块或共享函数。
6. 保持代码的一致性，不要在项目中维护多个具有相同功能的文件。
7. 打印日志时使用英文，不需要多语言支持，避免乱码。

关注Avalonia UI的版本变更：https://docs.avaloniaui.net/docs/stay-up-to-date/upgrade-from-0.10

### 代码优化建议

1. **UI/UX 一致性**
   - 确保所有窗口在主题切换、语言切换时有一致的行为

2. **模块化改进**
   - 将重复的主题和语言处理函数抽象为共享模块
   - 通过预加载脚本提供共享的类型和常量定义，避免重复定义

3. **错误处理**
   - 添加更全面的错误捕获和用户友好的错误显示
   - 实现操作日志记录功能

## Avalonia UI 常见错误及解决方案

在开发过程中遇到的一些常见错误和解决方案，可以作为参考：

1. **类型转换错误**
   - **错误描述**: 从 `IEnumerable<DataType>` 转换为 `IReadOnlyList<DataType>` 失败
   - **解决方案**: 使用 `.ToList()` 方法进行显式转换，例如：
     ```csharp
     // 转换为List以匹配需要的参数类型
     ViewModel.HandleDroppedItem(files.ToList(), this);
     ```

2. **XAML绑定错误**
   - **错误描述**: 无法解析转换器，如 `ObjectConverters.IsZero` 或 `BoolConverters.ToString`
   - **解决方案**: 
     - 使用内联表达式代替转换器，例如 `IsVisible="{Binding !FilteredItems.Count}"`
     - 对于标题等简单文本，可以使用静态文本而非复杂绑定

3. **DataTemplate绑定错误**
   - **错误描述**: 无法解析DataTemplate中的属性
   - **解决方案**: 
     - 显式声明DataTemplate的数据类型：`<DataTemplate x:DataType="vm:LauncherItemViewModel">`
     - 确保绑定到公共属性而非方法，如将 `{Binding GetIcon()}` 替换为 `{Binding Icon}`
     - 在ViewModel中添加相应的属性：`public string Icon => GetIcon();`

4. **ContextMenu绑定问题**
   - **错误描述**: ContextMenu默认不继承DataContext，导致绑定失败
   - **解决方案**: 使用特殊的绑定语法访问父元素的DataContext:
     ```xml
     <ContextMenu>
         <MenuItem Command="{Binding $parent[Window].DataContext.SomeCommand}" />
     </ContextMenu>
     ```

5. **编译时文件锁定**
   - **错误描述**: DLL被其他进程锁定，无法覆盖
   - **解决方案**: 
     - 使用 `dotnet clean` 清理项目后再构建
     - 关闭所有可能运行的应用实例
     - 重启IDE或开发工具

6. **ListBox控件命名错误**
   - **错误描述**: `Items` 属性不存在
   - **解决方案**: 在Avalonia中使用 `ItemsSource` 而非 `Items` 绑定集合数据

这些问题大多与Avalonia UI的特定实现方式有关，特别是与其他XAML框架（如WPF或UWP）的微小差异造成的。在Avalonia开发中，推荐经常参考[官方文档](https://docs.avaloniaui.net)以及示例项目。

## 通用操作流程

1. **添加新项目流程**
   - 用户点击添加按钮或拖放文件
   - 显示编辑窗口
   - 填写/确认项目信息
   - 保存项目到数据存储
   - 刷新主窗口列表
   - 更新托盘菜单

2. **打开项目流程**
   - 用户双击项目或右键菜单选择打开
   - 根据项目类型执行对应操作:
     - 文件/文件夹/URL: 使用系统默认程序打开
     - 命令: 使用平台特定方法在终端执行

3. **设置主题流程**
   - 用户在设置窗口选择主题
   - 保存主题设置到本地存储
   - 通知所有窗口应用新主题
   - 根据主题应用相应CSS类

4. **设置语言流程**
   - 用户在设置窗口选择语言
   - 保存语言设置
   - 加载对应语言资源
   - 更新所有窗口的文本
