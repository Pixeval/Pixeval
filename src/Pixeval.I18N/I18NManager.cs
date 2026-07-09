using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Pixeval.I18N;

public static class I18NManager
{
    private static ILangPlugin? _LangPlugin;

    public static List<string> CandidatePaths { get; }

    public static IReadOnlyDictionary<CultureInfo, DirectoryInfo> AvailableCultures { get; } = new Dictionary<CultureInfo, DirectoryInfo>();

    /// <summary>
    /// 注册插件并初始化默认文化的资源
    /// </summary>
    public static void Register(ILangPlugin plugin, CultureInfo defaultCulture)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ResolveResourceDirectory();
        plugin.Load(defaultCulture, AvailableCultures[defaultCulture], true);
        _LangPlugin = plugin;
    }

    /// <summary>
    /// 初始化当前文化资源
    /// </summary>
    public static void Initialize()
    {
        ArgumentNullException.ThrowIfNull(_LangPlugin);
        var currentCulture = CultureInfo.CurrentUICulture;
        if (!Equals(_LangPlugin.DefaultCulture, currentCulture))
            _LangPlugin.Load(currentCulture, AvailableCultures[currentCulture], false);
    }

    public static string GetResource(string key)
    {
        EnsureRegistered();
        return _LangPlugin.GetResource(key);
    }

    public static bool TryGetResource(string key, [NotNullWhen(true)] out string? value)
    {
        EnsureRegistered();
        if (_LangPlugin.CurrentCultureResources?.TryGetValue(key, out value) ?? false)
            return true;

        return _LangPlugin.DefaultCultureResources.TryGetValue(key, out value);
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

    private static void ResolveResourceDirectory()
    {
        var candidateDirectories = CandidatePaths
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => new DirectoryInfo(Path.Combine(path, "i18n")))
            .Where(t => t.Exists);

        var dictionary = (Dictionary<CultureInfo, DirectoryInfo>) AvailableCultures;

        foreach (var candidateDirectory in candidateDirectories)
        {
            foreach (var languageDirectory in candidateDirectory.EnumerateDirectories())
            {
                CultureInfo cultureInfo;
                try
                {
                    cultureInfo = CultureInfo.GetCultureInfo(languageDirectory.Name);
                }
                catch
                {
                    continue;
                }

                dictionary[cultureInfo] = languageDirectory;
            }
        }

        if (dictionary.Count is 0)
            throw new DirectoryNotFoundException($"Could not find the resource directory under {nameof(CandidatePaths)}.");
    }

    static I18NManager()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        CandidatePaths =
        [
            baseDirectory,
            AppContext.BaseDirectory,
            Environment.CurrentDirectory,
            Path.GetFullPath(Path.Combine(baseDirectory, "..")),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "Resources"))
        ];
    }
}
