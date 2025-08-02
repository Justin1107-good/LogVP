using LogVP.Models;
using System; 
using System.Text; 
using System.Windows;
using System.Windows.Controls; 
using System.Windows.Interop; 

namespace LogVP.Views.UserControls
{
    /// <summary>
    /// LogEntryDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class LogEntryDetailView : Window
    {
        private readonly LogEntry _logEntry;

        public LogEntryDetailView(LogEntry logEntry)
        {
            InitializeComponent();
            _logEntry = logEntry;
            LoadLogEntryDetails();
        }
        private void LoadLogEntryDetails()
        {
            if (_logEntry != null)
            {
                txtTime.Text = _logEntry.Time.ToString("yyyy-MM-dd HH:mm:ss.fff");
                txtLevel.Text = _logEntry.Level;
                txtSource.Text = _logEntry.Source;
                txtFileName.Text = _logEntry.FileName;
                txtMessage.Text = _logEntry.Message;
            }
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_logEntry.Message);
                MessageBox.Show("消息内容已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCopyAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"时间: {_logEntry.Time:yyyy-MM-dd HH:mm:ss.fff}");
                sb.AppendLine($"级别: {_logEntry.Level}");
                sb.AppendLine($"来源: {_logEntry.Source}");
                sb.AppendLine($"文件: {_logEntry.FileName}");
                sb.AppendLine($"消息: {_logEntry.Message}");

                Clipboard.SetText(sb.ToString());
                MessageBox.Show("全部内容已复制到剪贴板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        // 确保窗口始终在父窗口顶部
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            if (helper.Owner != IntPtr.Zero)
            {
                var parent = helper.Owner;
                var style = NativeMethods.GetWindowLong(parent, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, style | NativeMethods.WS_EX_TOPMOST);
            }
        }
    }
    internal static class NativeMethods
    {
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOPMOST = 0x00000008;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
