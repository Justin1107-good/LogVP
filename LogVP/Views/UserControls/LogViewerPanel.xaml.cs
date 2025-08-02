using LogVP.Models;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogVP.Views.UserControls
{
    /// <summary>
    /// LogViewerPanel.xaml 的交互逻辑
    /// </summary>
    public partial class LogViewerPanel : System.Windows.Controls.UserControl
    {
        private readonly ObservableCollection<LogEntry> _logEntries;
        private readonly ICollectionView _logView;
        private string _currentFilter = "";
        private CustomLogLevel _currentLogLevel = CustomLogLevel.All;
        private int _maxLogCount = 5000;
        private bool _isPaused = false;
        private readonly object _lockObject = new object();

        // 性能优化相关
        private const int BATCH_UPDATE_SIZE = 100;
        private System.Windows.Threading.DispatcherTimer _updateTimer;
        private readonly Queue<LogEntry> _pendingEntries = new Queue<LogEntry>();
        public LogViewerPanel()
        {
            InitializeComponent();
            _logEntries = new ObservableCollection<LogEntry>();
            _logView = CollectionViewSource.GetDefaultView(_logEntries);
            InitializeLogViewer();
        }
        private void InitializeLogViewer()
        {
            // 设置数据绑定
            dgLogs.ItemsSource = _logView;
            cmbLogLevel.SelectedIndex = 0;
            cmbMaxLogs.SelectedIndex = 1;

            // 初始化过滤器
            SetupFilter();

            // 初始化更新定时器
            _updateTimer = new System.Windows.Threading.DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(100);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            // 订阅NLog事件
            SubscribeToNLogEvents();
        }
        private void SubscribeToNLogEvents()
        {
            var config = LogManager.Configuration ?? new NLog.Config.LoggingConfiguration();

            var target = new LogViewerTarget(this);
            config.AddTarget("LogViewer", target);

            var rule = new NLog.Config.LoggingRule("*", LogLevel.Trace, target);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }
        private void SetupFilter()
        {
            _logView.Filter = item =>
            {
                if (item is LogEntry logEntry)
                {
                    // 级别过滤
                    bool levelMatch = _currentLogLevel == CustomLogLevel.All ||
                                     (int)_currentLogLevel <= logEntry.LogLevelOrdinal;

                    // 搜索过滤
                    bool searchMatch = string.IsNullOrEmpty(_currentFilter) ||

                                     logEntry.Message.IndexOf(_currentFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     logEntry.Source.IndexOf(_currentFilter, StringComparison.OrdinalIgnoreCase) >= 0;



                    return levelMatch && searchMatch;
                }
                return false;
            };
        }
        /// <summary>
        /// 线程安全地添加日志条目
        /// </summary>
        public void AddLogEntry(LogEntry entry)
        {
            if (_isPaused) return;

            lock (_lockObject)
            {
                _pendingEntries.Enqueue(entry);
            }
        }
        /// <summary>
        /// 批量更新UI以提高性能
        /// </summary>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            LogEntry[] entriesToProcess;

            lock (_lockObject)
            {
                if (_pendingEntries.Count == 0) return;

                int batchSize = Math.Min(BATCH_UPDATE_SIZE, _pendingEntries.Count);
                entriesToProcess = new LogEntry[batchSize];

                for (int i = 0; i < batchSize; i++)
                {
                    entriesToProcess[i] = _pendingEntries.Dequeue();
                }
            }

            // 在UI线程中更新
            Dispatcher.Invoke(() =>
            {
                foreach (var entry in entriesToProcess)
                {
                    AddEntryToCollection(entry);
                }

                // 应用大小限制
                ApplySizeLimit();

                // 更新显示
                UpdateDisplay();
            });
        }

        private void AddEntryToCollection(LogEntry entry)
        {
            _logEntries.Add(entry);
        }

        private void ApplySizeLimit()
        {
            if (_maxLogCount > 0 && _logEntries.Count > _maxLogCount)
            {
                int removeCount = _logEntries.Count - _maxLogCount;
                for (int i = 0; i < removeCount; i++)
                {
                    _logEntries.RemoveAt(0);
                }
            }
        }
        private void UpdateDisplay()
        {
            txtLogCount.Text = $"日志条数: {_logEntries.Count}";

            if (chkAutoScroll.IsChecked == true && _logEntries.Count > 0)
            {
                dgLogs.ScrollIntoView(_logEntries[_logEntries.Count - 1]);
            }
        }

        private void ApplyFilter()
        {
            _logView.Refresh();
            UpdateDisplay();
        }
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            lock (_lockObject)
            {
                _pendingEntries.Clear();
            }

            _logEntries.Clear();
            UpdateDisplay();
            txtStatus.Text = "日志已清空";
        }
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "文本文件 (*.txt)|*.txt|CSV文件 (*.csv)|*.csv|所有文件 (*.*)|*.*",
                    FileName = $"log_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    ExportLogs(saveDialog.FileName, saveDialog.FilterIndex);
                    txtStatus.Text = $"日志已导出到: {saveDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void ExportLogs(string fileName, int filterIndex)
        {
            await Task.Run(() =>
            {
                using (var writer = new StreamWriter(fileName))
                {
                    var entries = _logEntries.ToList();

                    if (filterIndex == 2) // CSV
                    {
                        writer.WriteLine("时间,级别,消息,来源");
                        foreach (var entry in entries)
                        {
                            writer.WriteLine($"\"{entry.Time}\",\"{entry.Level}\",\"{entry.Message.Replace("\"", "\"\"")}\",\"{entry.Source}\"");
                        }
                    }
                    else // TXT
                    {
                        foreach (var entry in entries)
                        {
                            writer.WriteLine($"[{entry.Time:yyyy-MM-dd HH:mm:ss.fff}] [{entry.Level}] {entry.Message} [{entry.Source}]");
                        }
                    }
                }
            });
        }
        private void CmbLogLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLogLevel.SelectedItem is ComboBoxItem item && item.Tag is string level)
            {
                switch (level)
                {
                    case "All": _currentLogLevel = CustomLogLevel.All; break;
                    case "Debug": _currentLogLevel = CustomLogLevel.Debug; break;
                    case "Info": _currentLogLevel = CustomLogLevel.Info; break;
                    case "Warn": _currentLogLevel = CustomLogLevel.Warn; break;
                    case "Error": _currentLogLevel = CustomLogLevel.Error; break;
                    case "Fatal": _currentLogLevel = CustomLogLevel.Fatal; break;
                }
                ApplyFilter();
            }
        }
        private void CmbMaxLogs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMaxLogs.SelectedItem is ComboBoxItem item && item.Tag is string maxCount)
            {
                _maxLogCount = int.Parse(maxCount);
                ApplySizeLimit();
                UpdateDisplay();
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentFilter = txtSearch.Text;
            ApplyFilter();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }
        private void ChkPause_Click(object sender, RoutedEventArgs e)
        {
            _isPaused = chkPause.IsChecked == true;
            txtStatus.Text = _isPaused ? "日志查看已暂停" : "日志查看已恢复";
        }

        // 新增：加载文件夹日志
        private async void BtnLoadFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                await LoadLogsFromFolder(folderDialog.SelectedPath);
            }
        }

        // 新增：加载单个文件
        private async void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "日志文件 (*.log;*.txt)|*.log;*.txt|所有文件 (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await LoadLogsFromFiles(openFileDialog.FileNames);
            }
        }
        // 加载文件夹中的所有日志文件
        private async Task LoadLogsFromFolder(string folderPath)
        {
            try
            {
                txtStatus.Text = "正在加载日志文件...";
                progressBar.Visibility = Visibility.Visible;
                // 使用 Progress<T> 报告进度
                var progress = new Progress<string>(status =>
                {
                    Dispatcher.Invoke(() => txtStatus.Text = status);
                });
                await Task.Run(async () =>
                {
                    var logFiles = Directory.GetFiles(folderPath, "*.log", SearchOption.AllDirectories)
                                          .Concat(Directory.GetFiles(folderPath, "*.txt", SearchOption.AllDirectories));

                    await LoadLogsFromFiles(logFiles.ToArray(), progress);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show($"加载文件夹失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    txtStatus.Text = "文件夹日志加载完成";
                    progressBar.Visibility = Visibility.Collapsed;
                });
            }
        }
        // 加载指定的日志文件
        private async Task LoadLogsFromFiles(string[] filePaths, IProgress<string> progress = null)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    txtStatus.Text = $"正在加载 {filePaths.Length} 个文件...";
                    progressBar.Visibility = Visibility.Visible;
                });
               

                int totalProcessed = 0;
                int totalFiles = filePaths.Length;

                foreach (var filePath in filePaths)
                {
                    await Task.Run(() =>
                    {
                        LoadLogFile(filePath);
                    });

                    totalProcessed++;
                    progress?.Report($"正在加载文件 ({totalProcessed}/{totalFiles}): {System.IO.Path.GetFileName(filePath)}");
                    //Dispatcher.Invoke(() =>
                    //{
                    //    txtStatus.Text = $"正在加载文件 ({totalProcessed}/{totalFiles}): {System.IO.Path.GetFileName(filePath)}";
                    //});
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show($"加载文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                progress?.Report("文件加载完成");
            }
        }
        // 解析并加载单个日志文件
        private void LoadLogFile(string filePath)
        {
            string fileName = System.IO.Path.GetFileName(filePath);
            try
            {

                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var logEntry = ParseLogLine(line, fileName);
                    if (logEntry != null)
                    {
                        AddLogEntry(logEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录解析错误但不中断整个过程
                var errorEntry = new LogEntry(new LogEventInfo(LogLevel.Error, "LogParser", $"解析文件 {filePath} 时出错: {ex.Message}"))
                {
                    FileName = fileName
                };
                AddLogEntry(errorEntry);
            }
        }
        // 解析单行日志
        private LogEntry ParseLogLine(string line, string fileName)
        {
            try
            {
                // 尝试解析常见的日志格式
                // 格式1: [2023-01-01 12:00:00.000] [INFO] Message [Source]
                var match = System.Text.RegularExpressions.Regex.Match(line,
                    @"\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})\]\s*\[([A-Z]+)\]\s*(.+?)(?:\s*\[(.+?)\])?$");

                if (match.Success)
                {
                    DateTime time = DateTime.Parse(match.Groups[1].Value);
                    string level = match.Groups[2].Value;
                    string message = match.Groups[3].Value;
                    string source = match.Groups[4].Value;

                    var logLevel = ParseLogLevel(level);
                    var logEvent = new LogEventInfo(logLevel, source ?? "File", message)
                    {
                        TimeStamp = time
                    };

                    return new LogEntry(logEvent) { FileName = fileName };
                }

                // 格式2: 2023-01-01 12:00:00.000 INFO Message
                match = System.Text.RegularExpressions.Regex.Match(line,
                    @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})\s+([A-Z]+)\s+(.+)");

                if (match.Success)
                {
                    DateTime time = DateTime.Parse(match.Groups[1].Value);
                    string level = match.Groups[2].Value;
                    string message = match.Groups[3].Value;

                    var logLevel = ParseLogLevel(level);
                    var logEvent = new LogEventInfo(logLevel, "File", message)
                    {
                        TimeStamp = time
                    };

                    return new LogEntry(logEvent) { FileName = fileName };
                }

                // 如果无法解析格式，创建一个通用条目
                var genericEvent = new LogEventInfo(LogLevel.Info, "File", line)
                {
                    TimeStamp = DateTime.Now
                };

                return new LogEntry(genericEvent) { FileName = fileName };
            }
            catch
            {
                // 如果解析失败，返回null
                return null;
            }
        }
        // 解析日志级别
        private LogLevel ParseLogLevel(string level)
        {

            switch (level.ToUpper())
            {
                case "TRACE":
                    return LogLevel.Trace;
                case "DEBUG":
                    return LogLevel.Debug;
                case "INFO":
                    return LogLevel.Info;
                case "WARN":
                case "WARNING":
                    return LogLevel.Warn;
                case "ERROR":
                    return LogLevel.Error;
                case "FATAL":
                    return LogLevel.Fatal;
                default:
                    return LogLevel.Info;
            }

        }

        private void DgLogs_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgLogs.SelectedItem is LogEntry selectedEntry)
            {
                ShowLogEntryDetail(selectedEntry);
            }
        }

        private void ShowLogEntryDetail(LogEntry logEntry)
        {
            try
            {
                var detailWindow = new LogEntryDetailView(logEntry);

                // 设置父窗口
                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    detailWindow.Owner = parentWindow;
                }

                detailWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"显示详情失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
