using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Windows.Storage;

namespace Pixeval.Logging;

public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _basePath = ApplicationData.Current.TemporaryFolder.Path + @"\Logs\";
    private readonly JsonSerializerOptions _options = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public FileLogger(string categoryName)
    {
        if (!Directory.Exists(_basePath))
            _ = Directory.CreateDirectory(_basePath);
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel is not LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel) || state?.ToString() is not { } logContent)
            return;

        var log = new LogModel(
            _categoryName,
            logLevel.ToString(),
            logContent,
            exception
        );

        var logStr = JsonSerializer.Serialize(log, _options);

        var logPath = _basePath + DateTime.Now.ToString("yyyyMMddHH") + ".log";

        var directory = Path.GetDirectoryName(logPath);
        _ = Directory.CreateDirectory(directory!);

        File.AppendAllText(logPath, logStr + Environment.NewLine, Encoding.UTF8);
    }
}

file class LogModel
{
    public LogModel(string category, string level, string message, Exception? exception)
    {
        Time = DateTime.Now;
        Category = category;
        Level = level;
        Message = message;
        if (exception is not null)
            Exception = new(exception);
    }

    public DateTime Time { get; }

    public string Category { get; }

    public string Level { get; }

    public string Message { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExceptionModel? Exception { get; }
}

file class ExceptionModel
{
    public ExceptionModel(Exception exception)
    {
        Source = exception.Source;
        Message = exception.Message;
        StackTrace = exception.StackTrace;
        if (exception.InnerException is not null)
            InnerException = new ExceptionModel(exception.InnerException);
    }

    public string? Source { get; }

    public string Message { get; }

    public string? StackTrace { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ExceptionModel? InnerException { get; }
}
