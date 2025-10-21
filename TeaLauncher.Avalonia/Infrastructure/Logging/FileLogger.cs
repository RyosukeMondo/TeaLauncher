using System;
using System.IO;
using System.Text;

namespace TeaLauncher.Avalonia.Infrastructure.Logging;

/// <summary>
/// Simple file-based logger for debugging and diagnostics.
/// Logs all activities to TeaLauncher.log in the application directory.
/// </summary>
public class FileLogger
{
    private static readonly object _lock = new object();
    private static FileLogger? _instance;
    private readonly string _logFilePath;
    private readonly bool _enabled;

    /// <summary>
    /// Gets the singleton instance of the logger.
    /// </summary>
    public static FileLogger Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new FileLogger();
                }
            }
            return _instance;
        }
    }

    private FileLogger()
    {
        try
        {
            // Log to the current directory where the exe is running
            _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "TeaLauncher.log");
            _enabled = true;

            // Write startup header
            WriteHeader();
        }
        catch (Exception)
        {
            // If we can't create the log file, disable logging
            _enabled = false;
        }
    }

    private void WriteHeader()
    {
        try
        {
            var header = new StringBuilder();
            header.AppendLine("========================================");
            header.AppendLine($"TeaLauncher Log - Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            header.AppendLine($"Process ID: {Environment.ProcessId}");
            header.AppendLine($"OS: {Environment.OSVersion}");
            header.AppendLine($".NET: {Environment.Version}");
            header.AppendLine($"Working Directory: {Directory.GetCurrentDirectory()}");
            header.AppendLine($"Executable: {Environment.ProcessPath}");
            header.AppendLine("========================================");

            File.WriteAllText(_logFilePath, header.ToString());
        }
        catch
        {
            // Silently fail if we can't write the header
        }
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public void Info(string message)
    {
        Log("INFO", message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public void Warning(string message)
    {
        Log("WARN", message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public void Error(string message)
    {
        Log("ERROR", message);
    }

    /// <summary>
    /// Logs an error with exception details.
    /// </summary>
    public void Error(string message, Exception ex)
    {
        var errorMessage = new StringBuilder();
        errorMessage.AppendLine(message);
        errorMessage.AppendLine($"Exception Type: {ex.GetType().FullName}");
        errorMessage.AppendLine($"Exception Message: {ex.Message}");
        errorMessage.AppendLine($"Stack Trace: {ex.StackTrace}");

        if (ex.InnerException != null)
        {
            errorMessage.AppendLine($"Inner Exception: {ex.InnerException.GetType().FullName}");
            errorMessage.AppendLine($"Inner Message: {ex.InnerException.Message}");
        }

        Log("ERROR", errorMessage.ToString());
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    public void Debug(string message)
    {
        Log("DEBUG", message);
    }

    private void Log(string level, string message)
    {
        if (!_enabled)
            return;

        try
        {
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var threadId = Environment.CurrentManagedThreadId;
                var logLine = $"[{timestamp}] [{level}] [Thread-{threadId}] {message}";

                File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
            }
        }
        catch
        {
            // Silently fail if we can't write to the log
        }
    }

    /// <summary>
    /// Writes a separator line in the log.
    /// </summary>
    public void Separator()
    {
        if (!_enabled)
            return;

        try
        {
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, "----------------------------------------" + Environment.NewLine);
            }
        }
        catch
        {
            // Silently fail
        }
    }

    /// <summary>
    /// Gets the log file path.
    /// </summary>
    public string GetLogFilePath() => _logFilePath;

    /// <summary>
    /// Checks if logging is enabled.
    /// </summary>
    public bool IsEnabled() => _enabled;
}
