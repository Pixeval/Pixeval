using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Pixeval.I18N;

public static class I18NManager
{
    private static ILangPlugin? _LangPlugin;

    public static void Register(ILangPlugin plugin, CultureInfo defaultCulture)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        _LangPlugin = plugin;
        var currentCulture = CultureInfo.CurrentUICulture;
        plugin.Load(currentCulture, defaultCulture);
    }

    public static string GetResource(string key)
    {
        EnsureRegistered();
        return _LangPlugin.GetResource(key);
    }

    public static string GetResource(string key, params object?[] args)
    {
        return string.Format(GetResource(key), args);
    }

    [MemberNotNull(nameof(_LangPlugin))]
    private static void EnsureRegistered()
    {
        if (_LangPlugin is null)
            throw new InvalidOperationException($"{nameof(I18NManager)} is not registered. Please register a language plugin before using it.");
    }
}
