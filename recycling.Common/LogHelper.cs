using System;
using System.IO;
using System.Text;

namespace recycling.Common
{
    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    /// <summary>
    /// 日志辅助类 - 提供简单的文件日志记录功能
    /// 注意：生产环境建议使用专业日志框架如Log4Net、NLog等
    /// </summary>
    public static class LogHelper
    {
        private static readonly object _lockObj = new object();
        private static string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static LogLevel _minLogLevel = LogLevel.Info;

        /// <summary>
        /// 设置日志目录
        /// </summary>
        /// <param name="directory">日志目录路径</param>
        public static void SetLogDirectory(string directory)
        {
            if (!string.IsNullOrWhiteSpace(directory))
            {
                _logDirectory = directory;
            }
        }

        /// <summary>
        /// 设置最低日志级别
        /// </summary>
        /// <param name="level">最低日志级别</param>
        public static void SetMinLogLevel(LogLevel level)
        {
            _minLogLevel = level;
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象（可选）</param>
        public static void Debug(string message, Exception exception = null)
        {
            Log(LogLevel.Debug, message, exception);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象（可选）</param>
        public static void Info(string message, Exception exception = null)
        {
            Log(LogLevel.Info, message, exception);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象（可选）</param>
        public static void Warning(string message, Exception exception = null)
        {
            Log(LogLevel.Warning, message, exception);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象（可选）</param>
        public static void Error(string message, Exception exception = null)
        {
            Log(LogLevel.Error, message, exception);
        }

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象（可选）</param>
        public static void Fatal(string message, Exception exception = null)
        {
            Log(LogLevel.Fatal, message, exception);
        }

        /// <summary>
        /// 核心日志记录方法
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        private static void Log(LogLevel level, string message, Exception exception)
        {
            // 检查日志级别
            if (level < _minLogLevel)
                return;

            try
            {
                // 确保日志目录存在
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }

                // 构建日志文件名（按日期分文件）
                string fileName = $"log_{DateTime.Now:yyyyMMdd}.txt";
                string filePath = Path.Combine(_logDirectory, fileName);

                // 构建日志内容
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}");

                // 如果有异常信息，追加异常详情
                if (exception != null)
                {
                    logBuilder.AppendLine($"Exception: {exception.GetType().Name}");
                    logBuilder.AppendLine($"Message: {exception.Message}");
                    logBuilder.AppendLine($"StackTrace: {exception.StackTrace}");

                    // 如果有内部异常，也记录
                    if (exception.InnerException != null)
                    {
                        logBuilder.AppendLine($"InnerException: {exception.InnerException.Message}");
                        logBuilder.AppendLine($"InnerStackTrace: {exception.InnerException.StackTrace}");
                    }
                }

                logBuilder.AppendLine(new string('-', 80)); // 分隔线

                // 使用锁确保线程安全
                lock (_lockObj)
                {
                    File.AppendAllText(filePath, logBuilder.ToString(), Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // 日志记录失败，输出到控制台（避免抛出异常）
                Console.WriteLine($"日志记录失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 清理旧日志文件
        /// </summary>
        /// <param name="daysToKeep">保留天数（默认30天）</param>
        public static void CleanOldLogs(int daysToKeep = 30)
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                    return;

                DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                string[] logFiles = Directory.GetFiles(_logDirectory, "log_*.txt");

                foreach (string file in logFiles)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        Info($"已删除过期日志文件：{fileInfo.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Error("清理旧日志文件失败", ex);
            }
        }

        /// <summary>
        /// 获取当前日志文件路径
        /// </summary>
        /// <returns>日志文件路径</returns>
        public static string GetCurrentLogFilePath()
        {
            string fileName = $"log_{DateTime.Now:yyyyMMdd}.txt";
            return Path.Combine(_logDirectory, fileName);
        }
    }
}
