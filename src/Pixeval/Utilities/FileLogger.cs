// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pixeval.Extensions.Common;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Pixeval.Utilities;

[GeneratedComClass]
public partial class FileLogger : ILogger, Pixeval.Extensions.Common.ILogger
{
    private readonly string _basePath;

    /// <summary>
    /// Return <see langword="true"/> to cancel the logging
    /// </summary>
    public event Func<FileLogger, LogModel, bool>? Logging;

    public FileLogger(string basePath)
    {
        _basePath = basePath;
        _ = FileHelper.TryCreateDirectory(_basePath);
    }

    private async Task LogPrivateAsync(
        LogLevel logLevel,
        string message,
        Exception? exception,
        string memberName,
        string filePath,
        int lineNumber)
    {
        try
        {
            // Logging is fire-and-forget, so it must never become a second unhandled exception.
            await Task.Yield();
            var networkDetails = exception is null
                ? null
                : await NetworkExceptionFormatter.TryFormatAsync(exception).ConfigureAwait(false);
            var now = DateTime.Now;
            var log = new LogModel(
                now,
                $"at {memberName} at {filePath}: {lineNumber}",
                logLevel.ToString(),
                message,
                exception,
                networkDetails);

            if (Logging is not null && Logging(this, log))
                return;

            var logPath = Path.Combine(_basePath, now.ToString("yyyy-MM-dd HH") + ".log");
            lock (this)
                File.AppendAllText(logPath, log.ToString(), Encoding.UTF8);
        }
        catch
        {
            // There is no safer fallback sink at this layer.
        }
    }

    public void Log(LogLevel logLevel, string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        _ = LogPrivateAsync(logLevel, message, exception, memberName, filePath, lineNumber);

    public void LogTrace(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        _ = LogPrivateAsync(LogLevel.Trace, message, exception, memberName, filePath, lineNumber);

    public void LogDebug(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        _ = LogPrivateAsync(LogLevel.Debug, message, exception, memberName, filePath, lineNumber);

    public void LogInformation(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        _ = LogPrivateAsync(LogLevel.Information, message, exception, memberName, filePath, lineNumber);

    public void LogWarning(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        _ = LogPrivateAsync(LogLevel.Warning, message, exception, memberName, filePath, lineNumber);

    public void LogError(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        _ = LogPrivateAsync(LogLevel.Error, message, exception, memberName, filePath, lineNumber);

    public void LogCritical(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        _ = LogPrivateAsync(LogLevel.Critical, message, exception, memberName, filePath, lineNumber);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        try
        {
            _ = LogPrivateAsync(logLevel, formatter(state, exception), exception, "", "", 0);
        }
        catch
        {
            // A logger must not propagate an exception from a third-party formatter.
        }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => throw new NotSupportedException();

    /// <inheritdoc />
    void Extensions.Common.ILogger.Log(Extensions.Common.LogLevel logLevel, string message, IException? exception, string memberName, string filePath, int lineNumber)
        => _ = LogPrivateAsync((LogLevel) logLevel, message, exception?.ToException(), memberName, filePath, lineNumber);
}

public class LogModel(DateTime time, string position, string level, string message, Exception? exception, string? networkDetails = null)
{
    public DateTime Time { get; } = time;

    public string Position { get; } = position;

    public string Level { get; } = level;

    public string Message { get; } = message;

    public ExceptionModel? Exception { get; } = exception is null ? null : new(exception);

    public string? NetworkDetails { get; } = networkDetails;

    public override string ToString() =>
        $"""
         {Time:HH:mm:ss} {Level}
         {Message}
         {Position}
         {Exception?.ToString(1)}
         {NetworkDetails}
         """;
}

public class ExceptionModel
{
    public ExceptionModel(Exception exception)
    {
        Exception = exception;
        Source = exception.Source;
        Message = exception.Message;
        StackTrace = exception.StackTrace;
        if (exception.InnerException is not null)
            InnerException = new ExceptionModel(exception.InnerException);
    }

    public Exception Exception { get; }

    public string? Source { get; }

    public string Message { get; }

    public string? StackTrace { get; }

    public ExceptionModel? InnerException { get; }

    public string ToString(int indent) =>
        $"""
         {Indent(indent)}Type: {Exception.GetType().FullName}
         {Indent(indent)}Source: {Source}
         {Indent(indent)}Message: {Message}
         {Indent(indent)}StackTrace: 
         {Indent(indent + 1)}{StackTrace?.ReplaceLineEndings(Environment.NewLine + Indent(indent + 1)) ?? "null"}
         {InnerException?.ToString(indent + 1)}

         """;

    private static string Indent(int indent) => new('\t', indent);
}
