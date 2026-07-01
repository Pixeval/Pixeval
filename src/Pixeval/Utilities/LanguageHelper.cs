// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
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

    /// <summary>
    /// 延迟加载，不能先于<see cref="I18NManager.Register"/>
    /// </summary>
    public static IReadOnlyList<CultureInfo> AvailableLanguages => field ??=
    [
        new DefaultCultureInfo(),
        ..I18NManager.AvailableCultures.Keys
    ];

    public static CultureInfo FindClosest(string? targetCulture)
    {
        var target = string.IsNullOrWhiteSpace(targetCulture) ? CultureInfo.CurrentUICulture : new CultureInfo(targetCulture);

        if (I18NManager.AvailableCultures.ContainsKey(target))
            return target;

        return I18NManager.AvailableCultures.Keys.MinBy(c => Distance(target, c))
               ?? throw new ArgumentNullException(nameof(I18NManager.AvailableCultures));
    }

    public static readonly CultureInfo DefaultLanguage = new("zh-Hans");
}

public class DefaultCultureInfo : CultureInfo
{
    /// <inheritdoc />
    public override string ToString() => "";

    /// <inheritdoc />
    public override string NativeName { get; } = I18NManager.GetResource(SettingsMainViewResources.LanguageSystemDefault);

    /// <inheritdoc />
    public override string DisplayName { get; } = I18NManager.GetResource(SettingsMainViewResources.LanguageSystemDefault);

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
