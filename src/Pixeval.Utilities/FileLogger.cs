// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Pixeval.Logging;

public class FileLogger : ILogger
{
    private readonly string _basePath;

    /// <summary>
    /// Return <see langword="true"/> to cancel the logging
    /// </summary>
    public event Func<FileLogger, LogModel, bool>? Logging;

    public FileLogger(string basePath)
    {
        _basePath = basePath;
        if (!Directory.Exists(_basePath))
            _ = Directory.CreateDirectory(_basePath);
    }

    private async void LogPrivate(LogLevel logLevel, string message, Exception? exception,
        string memberName,
        string filePath,
        int lineNumber)
    {
        var log = new LogModel(
            $"at {memberName} at {filePath}: {lineNumber}",
            logLevel.ToString(),
            message,
            exception);

        if (Logging is not null && Logging(this, log))
            return;

        var logPath = Path.Combine(_basePath, DateTime.Now.ToString("yyyy-MM-dd HH") + ".log");

        await Task.Yield();
        lock (this)
            File.AppendAllText(logPath, log.ToString(), Encoding.UTF8);
    }

    public void Log(LogLevel logLevel, string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        LogPrivate(logLevel, message, exception, memberName, filePath, lineNumber);

    public void LogTrace(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        LogPrivate(LogLevel.Trace, message, exception, memberName, filePath, lineNumber);

    public void LogDebug(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        LogPrivate(LogLevel.Debug, message, exception, memberName, filePath, lineNumber);

    public void LogInformation(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        LogPrivate(LogLevel.Information, message, exception, memberName, filePath, lineNumber);

    public void LogWarning(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        LogPrivate(LogLevel.Warning, message, exception, memberName, filePath, lineNumber);

    public void LogError(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        LogPrivate(LogLevel.Error, message, exception, memberName, filePath, lineNumber);

    public void LogCritical(string message, Exception? exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0) =>
        LogPrivate(LogLevel.Critical, message, exception, memberName, filePath, lineNumber);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        LogPrivate(logLevel, formatter(state, exception), exception, "", "", 0);

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => throw new NotSupportedException();
}

public class LogModel
{
    public LogModel(string position, string level, string message, Exception? exception)
    {
        Time = DateTime.Now;
        Position = position;
        Level = level;
        Message = message;
        if (exception is not null)
            Exception = new(exception);
    }

    public DateTime Time { get; }

    public string Position { get; }

    public string Level { get; }

    public string Message { get; }

    public ExceptionModel? Exception { get; }

    public override string ToString() =>
        $"""
         {Time:HH:mm:ss} {Level}
         {Message}
         {Position}
         {Exception?.ToString(1)}
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
