# LogViewPanel

# WPF日志查看面板使用说明教程

## 1. 项目概述

这是一个基于WPF和NLog的通用日志查看面板，支持实时日志显示、文件加载、搜索过滤、详细查看等功能。适用于任何需要日志监控和分析的.NET应用程序。

## 2. 功能特性

### 2.1 实时日志监控
- 自动捕获应用程序的NLog日志
- 支持所有日志级别（Debug/Fatal等）
- 实时显示新产生的日志

### 2.2 文件日志加载
- 支持加载单个日志文件
- 支持加载整个文件夹及其子文件夹中的日志
- 自动解析多种日志格式

### 2.3 搜索和过滤
- 按关键字搜索日志内容
- 按日志级别过滤显示
- 支持组合过滤条件

### 2.4 性能优化
- 使用虚拟化列表提高大数据量显示性能
- 批量更新机制避免UI阻塞
- 内存管理限制最大日志条数

### 2.5 详细信息查看
- 双击日志条目查看详细信息
- 支持复制日志内容到剪贴板

### 2.6 导出功能
- 支持导出为TXT或CSV格式
- 保留完整的日志信息

## 3. 安装和配置

### 3.1 环境要求
- .NET Framework 4.7.2 或更高版本
- Visual Studio 2019 或更高版本
- NLog NuGet包

### 3.2 NuGet包安装
```
Install-Package NLog
```

### 3.3 添加到项目
1. 将以下文件添加到您的WPF项目中：
   - `LogViewerPanel.xaml` 和 `LogViewerPanel.xaml.cs`
   - `LogEntryDetailView.xaml` 和 `LogEntryDetailView.xaml.cs`
   - `LogEntry.cs`
   - `LogViewerTarget.cs`

2. 在需要使用日志查看器的XAML文件中添加引用：
```xml
<local:LogViewerPanel x:Name="logViewer"/>
```

## 4. 使用方法

### 4.1 基本使用
```csharp
// 在MainWindow.xaml.cs中初始化NLog配置
private void ConfigureNLog()
{
    var config = new NLog.Config.LoggingConfiguration();
    
    // 控制台目标
    var consoleTarget = new NLog.Targets.ConsoleTarget("console");
    
    // 文件目标
    var fileTarget = new NLog.Targets.FileTarget("file")
    {
        FileName = "logs/log-${shortdate}.txt",
        Layout = "${longdate} ${level:uppercase=true} ${message} ${logger}"
    };
    
    config.AddTarget(consoleTarget);
    config.AddTarget(fileTarget);
    
    config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, consoleTarget);
    config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, fileTarget);
    
    NLog.LogManager.Configuration = config;
}
```

### 4.2 在界面中使用
```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:LogViewer.Views.UserControls"
        Title="日志查看器" Height="600" Width="1000">
    <Grid>
        <local:LogViewerPanel x:Name="logViewer"/>
    </Grid>
</Window>
```

## 5. 功能操作指南

### 5.1 工具栏功能

#### 清空按钮
- 点击"清空"按钮清除当前显示的所有日志
- 不会影响正在记录的日志文件

#### 导出按钮
- 点击"导出"按钮将当前显示的日志导出到文件
- 支持TXT和CSV两种格式
- 文件名自动包含时间戳

#### 加载文件夹
- 点击"加载文件夹"按钮选择包含日志文件的文件夹
- 自动递归搜索所有.log和.txt文件
- 支持多层级目录结构

#### 加载文件
- 点击"加载文件"按钮选择单个或多个日志文件
- 支持多选功能
- 支持.log和.txt文件格式

#### 自动滚动
- 勾选"自动滚动"使日志列表始终显示最新条目
- 取消勾选可暂停滚动，便于查看历史日志

#### 暂停
- 勾选"暂停"临时停止接收新的实时日志
- 不影响文件加载功能

### 5.2 日志级别过滤
1. 在级别下拉框中选择需要显示的日志级别
2. 支持的级别包括：
   - 全部：显示所有级别的日志
   - 调试(Debug)：显示Debug级别及以上日志
   - 信息(Info)：显示Info级别及以上日志
   - 警告(Warn)：显示Warn级别及以上日志
   - 错误(Error)：显示Error级别及以上日志
   - 致命(Fatal)：仅显示Fatal级别日志

### 5.3 最大条数限制
1. 在最大条数下拉框中选择日志显示的最大数量
2. 支持的选项：
   - 1000：最多显示1000条日志
   - 5000：最多显示5000条日志（默认）
   - 10000：最多显示10000条日志
   - 无限制：不进行数量限制

### 5.4 搜索功能
1. 在搜索框中输入关键字
2. 实时过滤包含关键字的日志条目
3. 搜索范围包括：消息内容、来源、文件名

### 5.5 详细信息查看
1. 在日志列表中双击任意一行
2. 弹出详细信息窗口
3. 窗口中显示：
   - 完整的时间戳
   - 日志级别
   - 来源信息
   - 文件名（如果适用）
   - 完整的消息内容

### 5.6 复制功能
在详细信息窗口中：
- 点击"复制消息"：仅复制消息内容
- 点击"复制全部"：复制完整的日志信息
- 复制成功后会显示提示信息

## 6. 日志格式支持

### 6.1 支持的格式
1. 标准NLog格式：`[2023-01-01 12:00:00.000] [INFO] Message [Source]`
2. 简化格式：`2023-01-01 12:00:00.000 INFO Message`
3. 通用格式：无法识别的行作为普通消息处理

### 6.2 自动识别信息
- 时间戳：自动解析并显示
- 日志级别：自动识别Trace/Debug/Info/Warn/Error/Fatal
- 消息内容：完整保留
- 来源信息：如果有则显示

## 7. 性能优化说明

### 7.1 虚拟化显示
- 使用DataGrid虚拟化功能
- 只渲染可见的日志条目
- 支持大量数据流畅滚动

### 7.2 批量更新
- 实时日志采用批量处理机制
- 每100毫秒批量更新一次UI
- 避免频繁的UI线程调用

### 7.3 内存管理
- 可配置最大日志条数
- 自动清理超出限制的旧日志
- 防止内存泄漏

## 8. 常见问题解答

### 8.1 为什么看不到实时日志？
- 确保项目中正确配置了NLog
- 检查是否有其他NLog目标冲突
- 确认应用程序确实在产生日志

### 8.2 加载文件很慢怎么办？
- 检查文件大小，过大的文件建议分批加载
- 确保磁盘读取性能正常
- 可以暂停实时日志接收来提高加载性能

### 8.3 搜索功能不工作？
- 确认搜索关键字拼写正确
- 搜索是实时进行的，输入后立即生效
- 搜索范围包括消息、来源和文件名

### 8.4 详细窗口无法显示？
- 检查是否有安全软件阻止弹窗
- 确认主窗口是否正常显示
- 重启应用程序尝试

## 9. 自定义扩展

### 9.1 添加新的日志级别
修改 `CustomLogLevel` 枚举添加新的级别。

### 9.2 支持更多文件格式
修改 `ParseLogLine` 方法添加新的解析规则。

### 9.3 修改界面样式
直接编辑XAML文件调整界面布局和样式。

### 9.4 添加新的导出格式
在 `ExportLogs` 方法中添加新的导出逻辑。

## 10. 技术支持

如遇到问题，请检查：
1. .NET Framework版本是否符合要求
2. NLog包是否正确安装
3. 文件权限是否正常
4. 磁盘空间是否充足

这个日志查看面板提供了完整的日志管理和分析功能，可以帮助开发者更好地监控和调试应用程序。