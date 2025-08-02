using LogVP.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogVP
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public MainWindow()
        {
            InitializeComponent();
            // 配置NLog
            ConfigureNLog();

        }
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
        private void BtnTestLog_Click(object sender, RoutedEventArgs e)
        {
            

            List<LogEventInfo> logs = new List<LogEventInfo> {

            new LogEventInfo
            {
                TimeStamp = DateTime.Now,
                Level = LogLevel.Info,
                Message = "这是一条普通信息",
                LoggerName = this.Title
            },
            new LogEventInfo
            {
                TimeStamp = DateTime.Now,
                Level = LogLevel.Debug,
                Message = "这是一条调试信息",
                LoggerName = this.Title
            }
            ,
            new LogEventInfo
            {
                TimeStamp = DateTime.Now,
                Level = LogLevel.Debug,
                Message = "这是一条警告信息",
                LoggerName = this.Title
            },
            new LogEventInfo
            {
                TimeStamp = DateTime.Now,
                Level = LogLevel.Error,
                Message = "这是一条错误信息",
                LoggerName = this.Title
            },
            new LogEventInfo
            {
                TimeStamp = DateTime.Now,
                Level = LogLevel.Fatal,
                Message = "这是一条致命错误信息",
                LoggerName = this.Title
            }
            };

            foreach (var item in logs)
            {
                logViewer.AddLogEntry(new LogEntry(item));
            }
           
        }
    }
}
