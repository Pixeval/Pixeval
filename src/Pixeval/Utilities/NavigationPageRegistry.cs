// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using FluentIcons.Common;
using Pixeval.Models.Navigation;

namespace Pixeval.Utilities;

public static class NavigationPageRegistry
{
    static NavigationPageRegistry()
    {
        AvaloniaHelper.Init();
    }

    public static List<NavigationPageDefinition> Pages { get; } = [];

    // 延迟到构造函数外赋值，确保Pages已赋值
    public static IReadOnlyDictionary<string, NavigationPageDefinition> PagesByKey => field ??=
        Pages.ToFrozenDictionary(static definition => definition.Key, StringComparer.OrdinalIgnoreCase);

    private static readonly Lazy<IReadOnlyList<Symbol>> _ColorSymbols = new(GetColorSymbols);

    public static IReadOnlyList<Symbol> ColorSymbols => _ColorSymbols.Value;

    public static bool TryGetPage(string key, [NotNullWhen(true)] out NavigationPageDefinition? definition) =>
        PagesByKey.TryGetValue(key, out definition);

    private static IReadOnlyList<Symbol> GetColorSymbols()
    {
        if (!FontManager.Current.TryGetGlyphTypeface(GetFluent(), out var glyphTypeface))
            return [];

        return
        [
            .. Enum.GetValues<Symbol>()
                .Where(symbol => ContainsGlyph(glyphTypeface, ToString(null, symbol, IconVariant.Color)))
                .OrderBy(t => t.ToString())
        ];
    }

    private static bool ContainsGlyph(GlyphTypeface glyphTypeface, string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        foreach (var rune in text.EnumerateRunes())
            if (!glyphTypeface.CharacterToGlyphMap.TryGetGlyph(rune.Value, out var glyphId)
                || glyphId is 0
                || !glyphTypeface.TryGetGlyphMetrics(glyphId, out _))
                return false;

        return true;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(GetFluent))]
    private static extern Typeface GetFluent([UnsafeAccessorType("FluentIcons.Resources.Avalonia.TypefaceManager, FluentIcons.Resources.Avalonia")] object? _ = null, IconSize size = IconSize.Resizable, IconVariant variant = IconVariant.Color);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(ToString))]
    private static extern string ToString<T>([UnsafeAccessorType("FluentIcons.Common.Internals.Extensions, FluentIcons.Common")] object? _, T value, IconVariant iconVariant, bool isRtl = false)
        where T : struct, Enum;
}
