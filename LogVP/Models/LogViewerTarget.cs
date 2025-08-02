using LogVP.Views.UserControls;
using NLog;
using NLog.Targets; 

namespace LogVP.Models
{
    [Target("LogViewer")]
    public class LogViewerTarget : TargetWithLayout
    {
        private readonly LogViewerPanel _logViewer;

        public LogViewerTarget(LogViewerPanel logViewer)
        {
            _logViewer = logViewer;
            Name = "LogViewer";
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var logEntry = new LogEntry(logEvent);
            _logViewer.AddLogEntry(logEntry);
        }
    }
    public enum CustomLogLevel
    {
        All = -1, // 表示所有级别
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5
    }
}
