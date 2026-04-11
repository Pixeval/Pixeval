using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Pixeval.I18N;

public static class I18NManager
{
    private static ILangPlugin? _LangPlugin;

    /// <summary>
    /// 注册插件并初始化默认文化的资源
    /// </summary>
    public static void Register(ILangPlugin plugin, CultureInfo defaultCulture)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ResolveResourceDirectory();
        plugin.Load(defaultCulture, CheckCultureFolderExists(defaultCulture), true);
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
            _LangPlugin.Load(currentCulture, CheckCultureFolderExists(currentCulture), false);
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

    public static DirectoryInfo ResourceFolder { get; private set; } = null!;

    private static void ResolveResourceDirectory()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var candidatePaths = new[]
            {
                Path.Combine(baseDirectory, "i18n"),
                Path.Combine(AppContext.BaseDirectory, "i18n"),
                Path.Combine(Environment.CurrentDirectory, "i18n"),
                Path.GetFullPath(Path.Combine(baseDirectory, "..", "i18n")),
                Path.GetFullPath(Path.Combine(baseDirectory, "..", "Resources", "i18n"))
            }
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var directory = candidatePaths
            .Where(Directory.Exists)
            .Select(candidatePath => new DirectoryInfo(candidatePath))
            .FirstOrDefault();
        ResourceFolder = directory ?? throw new DirectoryNotFoundException($"Could not find the resource directory under {baseDirectory}.");
    }

    private static DirectoryInfo CheckCultureFolderExists(CultureInfo culture)
    {
        var path = Path.Combine(ResourceFolder.FullName, culture.Name);
        var directory = new DirectoryInfo(path);
        return directory.Exists ? directory : throw new DirectoryNotFoundException($"Could not find the resource directory for culture {culture.Name} under {ResourceFolder.FullName}.");
    }
}
