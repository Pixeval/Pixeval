// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace Pixeval.Utilities;

internal static class FontFamilyHelper
{
    public static FontFamily? Create(string? fontFamilyName)
    {
        if (string.IsNullOrWhiteSpace(fontFamilyName))
            return null;

        var fontFamily = new FontFamily(fontFamilyName);
        return IsUsable(fontFamily) ? fontFamily : null;
    }

    public static FontFamily? Create(IEnumerable<string> fontFamilyNames)
    {
        var fontFamilyName = string.Join(',', fontFamilyNames
            .Select(Create)
            .OfType<FontFamily>()
            .Select(static fontFamily => fontFamily.Name));
        return string.IsNullOrWhiteSpace(fontFamilyName) ? null : new FontFamily(fontFamilyName);
    }

    public static bool IsUsable(FontFamily fontFamily)
    {
        try
        {
            // Avalonia normalizes installed family names during layout, where the exception can no longer be caught here.
            _ = new Typeface(fontFamily).Normalize(out _);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
