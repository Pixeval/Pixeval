using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Pixeval.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder)
    {
        _ = builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
        return builder;
    }
}
