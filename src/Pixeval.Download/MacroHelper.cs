using System.IO;
using System.Linq;

namespace Pixeval.Download;

internal static class MacroHelper
{
    public static string InvalidNameCharsInMacro { get; } = @"<>\/*:?""|" + new string(Path.GetInvalidPathChars());

    public static string NormalizePathSegmentInMacro(string path)
    {
        return InvalidNameCharsInMacro.Aggregate(path, (s, c) => s.Replace(c.ToString(), "")).TrimEnd('.');
    }
}
