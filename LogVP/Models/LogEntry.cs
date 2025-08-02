using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogVP.Models
{
    /// <summary>
    /// 日志条目实体类，实现INotifyPropertyChanged以支持数据绑定
    /// </summary>
    public class LogEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime _time;
        private string _level;
        private int _logLevelOrdinal;
        private string _message;
        private string _source;
        private string _fileName;
        public DateTime Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        public string Level
        {
            get => _level;
            set
            {
                _level = value;
                OnPropertyChanged(nameof(Level));
            }
        }

        public int LogLevelOrdinal
        {
            get => _logLevelOrdinal;
            set
            {
                _logLevelOrdinal = value;
                OnPropertyChanged(nameof(LogLevelOrdinal));
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public string Source
        {
            get => _source;
            set
            {
                _source = value;
                OnPropertyChanged(nameof(Source));
            }
        }
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }
        public LogEntry(LogEventInfo logEvent)
        {
            Time = logEvent.TimeStamp;
            Level = logEvent.Level.ToString();
            LogLevelOrdinal = logEvent.Level.Ordinal; // 将 NLog 的 LogLevel 转换为整数
            Message = logEvent.FormattedMessage;
            Source = logEvent.LoggerName ?? "Unknown";
            FileName = string.Empty;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
