using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Pixeval.Download;

namespace Pixeval.Models.Download.Macros;

internal static class MacroHelper
{
    public static bool IsStringFormatterValid(string? formatter) => formatter is null or "u" or "l";

    public static string FormatString(string value, string? formatter) =>
        formatter switch
        {
            "u" => value.ToUpperInvariant(),
            "l" => value.ToLowerInvariant(),
            _ => value
        };

    public static bool IsIntegerFormatterValid(string? formatter)
    {
        if (formatter is null)
            return true;

        if (formatter.Length is 0 || formatter.Any(MetaPathParserHelper.InvalidNameCharsInMacro.Contains))
            return false;

        try
        {
            _ = 0.ToString(formatter, CultureInfo.InvariantCulture);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string FormatInteger(int value, string? formatter) =>
        formatter is null
            ? value.ToString(CultureInfo.InvariantCulture)
            : value.ToString(formatter, CultureInfo.InvariantCulture);

    public static bool IsDateTimeOffsetFormatterValid(string? formatter)
    {
        if (formatter is null)
            return true;

        if (formatter.Length is 0 || formatter.Any(MetaPathParserHelper.InvalidNameCharsInMacro.Contains))
            return false;

        try
        {
            _ = DateTimeOffset.MinValue.ToString(formatter, CultureInfo.InvariantCulture);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string FormatDateTimeOffset(DateTimeOffset value, string formatter)
    {
        // 默认格式可能包含时分秒或者其他不适合文件名的字符，调用方法前最好使用 yyyy-M-d 作为默认兜底
        Debug.Assert(formatter is not null);
        return value.ToString(formatter, CultureInfo.InvariantCulture);
    }

    public static string CreateToken(string name, string? formatter) =>
        formatter is null ? $"<{name}>" : $"<{name}:{formatter}>";
}
