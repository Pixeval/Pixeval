// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pixeval.I18N;

namespace Pixeval.Utilities;

public static class LanguageHelper
{
    private static int Distance(CultureInfo target, CultureInfo candidate)
    {
        if (target.TwoLetterISOLanguageName != candidate.TwoLetterISOLanguageName)
            return int.MaxValue / 2; // 不同主语言，视为很远

        var d = 0;
        for (var p = target; !Equals(p, CultureInfo.InvariantCulture); p = p.Parent, d++)
        {
            if (Equals(p, candidate))
                return d; // target 到 candidate 的父链距离
        }

        // candidate 可能是 target 的子文化（不常见），再反向找
        d = 0;
        for (var p = candidate; !Equals(p, CultureInfo.InvariantCulture); p = p.Parent, d++)
        {
            if (Equals(p, target))
                return d;
        }

        return int.MaxValue / 2;
    }

    public static CultureInfo FindClosest(string? targetCulture)
    {
        var target = targetCulture is null ? CultureInfo.CurrentUICulture : new CultureInfo(targetCulture);

        if (Presets.Contains(target))
            return target;

        return Presets.MinBy(c => Distance(target, c))
            ?? throw new ArgumentNullException(nameof(Presets));
    }

    public static IReadOnlyList<CultureInfo> AvailableLanguages => field ??=
    [
        new DefaultCultureInfo(),
        ..Presets
    ];

    private static FrozenSet<CultureInfo> Presets => field ??= new HashSet<CultureInfo>
    {
        DefaultLanguage,
        new CultureInfo("ru-RU"),
        new CultureInfo("fr-FR"),
        new CultureInfo("en-US"),
    }.ToFrozenSet();

    public static readonly CultureInfo DefaultLanguage = new("zh-Hans");
}

public class DefaultCultureInfo : CultureInfo
{
    /// <inheritdoc />
    public override string ToString() => "";

    /// <inheritdoc />
    public override string NativeName { get; } = I18NManager.GetResource(SettingsPageResources.LanguageSystemDefault);

    /// <inheritdoc />
    public override string DisplayName { get; } = I18NManager.GetResource(SettingsPageResources.LanguageSystemDefault);

    /// <inheritdoc />
    public DefaultCultureInfo() : base("")
    {
    }

    /// <inheritdoc />
    private DefaultCultureInfo(int culture) : base(culture)
    {
    }

    /// <inheritdoc />
    private DefaultCultureInfo(int culture, bool useUserOverride) : base(culture, useUserOverride)
    {
    }

    /// <inheritdoc />
    private DefaultCultureInfo(string name, bool useUserOverride) : base(name, useUserOverride)
    {
    }
}
