using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Pixeval.I18N;

public interface ILangPlugin
{
    internal Dictionary<string, string> DefaultCultureResources { get; }

    internal Dictionary<string, string>? CurrentCultureResources { get; }

    IReadOnlyList<CultureInfo> AvailableCultures { get; }

    void Load(CultureInfo currentCulture, CultureInfo defaultCulture);

    CultureInfo CurrentCulture { get; }

    CultureInfo DefaultCulture { get; }

    string GetResource(string key)
    {
        EnsureLoaded();

        return CurrentCultureResources?.TryGetValue(key, out var value) ?? false
            ? value
            : DefaultCultureResources.GetValueOrDefault(key, "");
    }

    [MemberNotNull(nameof(CurrentCulture))]
    internal void EnsureLoaded()
    {
        if (CurrentCulture is null)
            throw new InvalidOperationException($"Please call {nameof(Load)} method before using the plugin.");
    }
}
