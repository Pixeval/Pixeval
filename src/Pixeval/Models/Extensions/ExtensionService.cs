// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.AppManagement;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.Extensions.Common.Downloaders;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Utilities;

namespace Pixeval.Models.Extensions;

public enum ExtensionHostLoadResult
{
    Loaded,
    NativeLibraryLoadFailed,
    MissingEntryPoint,
    EntryPointInvocationFailed,
    OutdatedSdk,
    InitializationFailed,
    ExtensionLoadFailed
}

public class ExtensionService : IDisposable
{
    public static string CurrentVersion { get; } = ExtensionsHostStatics.CurrentSdkVersion.ToString();

    public static string? NativeLibraryExtension
    {
        get
        {
            if (OperatingSystem.IsWindows())
                return ".dll";

            if (OperatingSystem.IsLinux() || OperatingSystem.IsAndroid())
                return ".so";

            if (OperatingSystem.IsMacOS() || OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst())
                return ".dylib";

            return null;
        }
    }

    public ObservableCollection<ExtensionsHostModel> HostModels { get; } = [];

    public IEnumerable<ExtensionsHostModel> ActiveModels => HostModels.Where(t => t.IsActive);

    public IReadOnlyList<ExtensionSettingsGroup> SettingsGroups => _settingsGroups;

    public IEnumerable<IExtension> Extensions => HostModels.SelectMany(t => t.Extensions);

    public IEnumerable<IExtension> ActiveExtensions => ActiveModels.SelectMany(t => t.Extensions);

    public IEnumerable<IImageTransformerCommandExtension> ActiveImageTransformerCommands => ActiveExtensions.OfType<IImageTransformerCommandExtension>();

    public IEnumerable<ITextTransformerCommandExtension> ActiveTextTransformerCommands => ActiveExtensions.OfType<ITextTransformerCommandExtension>();

    public IEnumerable<IDownloaderExtension> ActiveDownloaders => ActiveExtensions.OfType<IDownloaderExtension>();

    public IEnumerable<IStaticImageFormatProviderExtension> ActiveStaticImageFormatProviders => ActiveExtensions.OfType<IStaticImageFormatProviderExtension>();

    public IEnumerable<IAnimatedImageFormatProviderExtension> ActiveAnimatedImageFormatProviders => ActiveExtensions.OfType<IAnimatedImageFormatProviderExtension>();

    public IEnumerable<INovelFormatProviderExtension> ActiveNovelFormatProviders => ActiveExtensions.OfType<INovelFormatProviderExtension>();

    public IStaticImageFormatProviderExtension? GetStaticImageFormatProvider(string extension) =>
        ActiveStaticImageFormatProviders.FirstOrDefault(t => string.Equals(t.FormatExtension, extension, StringComparison.OrdinalIgnoreCase));

    public IAnimatedImageFormatProviderExtension? GetAnimatedImageFormatProvider(string extension) =>
        ActiveAnimatedImageFormatProviders.FirstOrDefault(t => string.Equals(t.FormatExtension, extension, StringComparison.OrdinalIgnoreCase));

    public INovelFormatProviderExtension? GetNovelFormatProvider(string extension) =>
        ActiveNovelFormatProviders.FirstOrDefault(t => string.Equals(t.FormatExtension, extension, StringComparison.OrdinalIgnoreCase));

    private readonly List<ExtensionSettingsGroup> _settingsGroups = [];

    private readonly Dictionary<string, Dictionary<string, object?>> _extensionSettings;

    public int OutDateExtensionHostsCount { get; }

    public ExtensionService(
        FileLogger logger,
        Dictionary<string, Dictionary<string, object?>> extensionSettings,
        bool loadInstalledHosts = true)
    {
        _extensionSettings = extensionSettings;

        if (!loadInstalledHosts)
            return;

        foreach (var library in EnumerateLocalExtensionHosts(AppInfo.ExtensionsFolder))
        {
            _ = TryLoadHost(library, logger, out var outdatedVersion);
            if (outdatedVersion is not null)
                ++OutDateExtensionHostsCount;
        }

        HostModels.CollectionChanged += (s, _) =>
        {
            if (s is ObservableCollection<ExtensionsHostModel> { Count: var c } o)
                for (var i = 0; i < c; i++)
                    o[i].Priority = i;
        };
    }

    public bool TryLoadHost(string path, ILogger logger, out string? outdatedVersion)
    {
        var result = TryLoadHostWithResult(path, logger, out _, out outdatedVersion);
        return result is ExtensionHostLoadResult.Loaded;
    }

    public static IEnumerable<string> EnumerateLocalExtensionHosts(string directory)
    {
        if (!Directory.Exists(directory) || NativeLibraryExtension is not { } extension)
            return [];

        var pattern = "*" + extension;

        var rootLibraries = Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly);
        var childLibraries = Directory.GetDirectories(directory)
            .SelectMany(t => Directory.GetFiles(t, pattern, SearchOption.TopDirectoryOnly));

        return rootLibraries.Concat(childLibraries)
            .Where(IsExtensionHostNativeLibrary)
            .Order(StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<string> GetExtensionHostEntryNames(ZipArchive zipArchive) =>
        [.. zipArchive.Entries
            .Where(IsSupportedRelativeNativeLibraryPath)
            .Where(IsExtensionHostNativeLibrary)
            .Select(t => Path.Combine(t.FullName.Split('/', '\\')))
            .Order(StringComparer.OrdinalIgnoreCase)];

    public static (string DestinationDirectory, IReadOnlyList<string> HostLibraryEntryNames) CreateExtensionZipExtractionPlan(
        ZipArchive zipArchive,
        string zipFilePath,
        string extensionsFolder)
    {
        var destinationDirectory = ContainsSingleTopLevelDirectory(zipArchive)
            ? extensionsFolder
            : Path.Combine(extensionsFolder, GetZipFolderName(zipFilePath));
        return (destinationDirectory, GetExtensionHostEntryNames(zipArchive));

        static bool ContainsSingleTopLevelDirectory(ZipArchive zipArchive)
        {
            string? rootDirectory = null;
            var hasEntry = false;
            foreach (var entry in zipArchive.Entries)
            {
                if (!TryGetZipEntrySegments(entry.FullName, out var segments))
                    return false;

                hasEntry = true;
                rootDirectory ??= segments[0];
                if (!string.Equals(rootDirectory, segments[0], StringComparison.OrdinalIgnoreCase))
                    return false;

                if (segments.Length is 1 && !IsDirectoryEntry(entry))
                    return false;
            }

            return hasEntry;

            static bool IsDirectoryEntry(ZipArchiveEntry entry) =>
                entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\');
        }

        static string GetZipFolderName(string zipFilePath) =>
            Path.GetFileNameWithoutExtension(zipFilePath) is { Length: > 0 } name
                ? name
                : "Extension";
    }

    public ExtensionHostLoadResult TryLoadHostWithResult(string path, ILogger logger, out ExtensionsHostModel? model, out string? outdatedVersion)
    {
        model = null;
        var result = LoadHost(path, logger, _extensionSettings, out var loadedModel, out outdatedVersion);
        if (result is not ExtensionHostLoadResult.Loaded)
            return result;

        try
        {
            LoadExtensions(loadedModel!);
            InsertHost(loadedModel!);
            model = loadedModel;
            return ExtensionHostLoadResult.Loaded;
        }
        catch
        {
            loadedModel?.Dispose();
            return ExtensionHostLoadResult.ExtensionLoadFailed;
        }
    }

    public void UnloadHost(ExtensionsHostModel model)
    {
        try
        {
            _ = HostModels.Remove(model);
            if (_settingsGroups.FirstOrDefault(t => t.Model == model) is { } group)
                _ = _settingsGroups.Remove(group);
            foreach (var extension in model.Extensions)
                extension.OnExtensionUnloaded();
            model.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    private static ExtensionHostLoadResult LoadHost(
        string path,
        ILogger logger,
        Dictionary<string, Dictionary<string, object?>> extensionSettings,
        out ExtensionsHostModel? model,
        out string? outdatedVersion)
    {
        model = null;
        outdatedVersion = null;
        try
        {
            if (!NativeLibrary.TryLoad(
                path,
                typeof(ExtensionService).Assembly,
                DllImportSearchPath.UseDllDirectoryForDependencies | DllImportSearchPath.SafeDirectories,
                out var libraryHandle))
                return ExtensionHostLoadResult.NativeLibraryLoadFailed;
            try
            {
                if (!NativeLibrary.TryGetExport(libraryHandle, nameof(GetExtensionsHost), out var getExtensionsHostPtr))
                {
                    NativeLibrary.Free(libraryHandle);
                    return ExtensionHostLoadResult.MissingEntryPoint;
                }
                var getExtensionsHost = Marshal.GetDelegateForFunctionPointer<GetExtensionsHost>(getExtensionsHostPtr);
                var result = getExtensionsHost(out var ppv);
                if (result is not 0)
                {
                    NativeLibrary.Free(libraryHandle);
                    return ExtensionHostLoadResult.EntryPointInvocationFailed;
                }
                var wrappers = new StrategyBasedComWrappers();
                var rcw = (IExtensionsHost) wrappers.GetOrCreateObjectForComInstance(ppv, CreateObjectFlags.UniqueInstance);
                _ = Marshal.Release(ppv);

                if (rcw.SdkVersion != CurrentVersion)
                {
                    NativeLibrary.Free(libraryHandle);
                    outdatedVersion = rcw.SdkVersion;
                    return ExtensionHostLoadResult.OutdatedSdk;
                }
                rcw.Initialize(CultureInfo.CurrentCulture.Name, AppInfo.TempFolder, Path.GetDirectoryName(path) ?? AppInfo.ExtensionsFolder, logger);
                model = new(rcw, GetValues(extensionSettings, rcw.ExtensionName)) { Handle = libraryHandle };
                return ExtensionHostLoadResult.Loaded;
            }
            catch
            {
                try
                {
                    NativeLibrary.Free(libraryHandle);
                }
                catch
                {
                    // ignored
                }
                return ExtensionHostLoadResult.InitializationFailed;
            }
        }
        catch
        {
            return ExtensionHostLoadResult.NativeLibraryLoadFailed;
        }

        static Dictionary<string, object?> GetValues(Dictionary<string, Dictionary<string, object?>> extensionSettings, string hostName)
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(extensionSettings, hostName, out var exists);
            if (!exists)
                value = [];
            return value!;
        }
    }

    private static ReadOnlySpan<byte> ExtensionHostEntryPointName => "GetExtensionsHost"u8;

    private static bool IsExtensionHostNativeLibrary(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return ContainsExtensionHostEntryPointName(stream);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsExtensionHostNativeLibrary(ZipArchiveEntry entry)
    {
        try
        {
            using var stream = entry.Open();
            return ContainsExtensionHostEntryPointName(stream);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsSupportedRelativeNativeLibraryPath(ZipArchiveEntry entry)
    {
        if (NativeLibraryExtension is not { } extension)
            return false;

        if (!TryGetZipEntrySegments(entry.FullName, out var segments))
            return false;

        return segments.Length is 1 or 2
            && string.Equals(Path.GetExtension(segments[^1]), extension, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetZipEntrySegments(string path, out string[] segments)
    {
        segments = [];
        if (path.Length is 0 || path.StartsWith('/') || path.StartsWith('\\'))
            return false;

        var trimmedPath = path.TrimEnd('/', '\\');
        if (trimmedPath.Length is 0)
            return false;

        segments = trimmedPath.Split('/', '\\');
        return segments.All(t => t is { Length: > 0 } and not "." and not ".." && !t.Contains(':'));
    }

    private static bool ContainsExtensionHostEntryPointName(Stream stream)
    {
        var entryPointName = ExtensionHostEntryPointName;
        Span<byte> buffer = stackalloc byte[4096 + 20];
        var retained = 0;

        while (true)
        {
            var read = stream.Read(buffer[retained..]);
            if (read is 0)
                return false;

            var length = retained + read;
            if (buffer[..length].IndexOf(entryPointName) >= 0)
                return true;

            retained = int.Min(entryPointName.Length - 1, length);
            buffer[(length - retained)..length].CopyTo(buffer);
        }
    }

    private void InsertHost(ExtensionsHostModel model)
    {
        var inserted = false;
        for (var i = 0; i < HostModels.Count; ++i)
            if (HostModels[i].Priority >= model.Priority)
            {
                HostModels.Insert(i, model);
                inserted = true;
                break;
            }

        if (!inserted)
            HostModels.Add(model);
    }

    private void LoadExtensions(ExtensionsHostModel hostModel)
    {
        var ext = hostModel.Extensions;
        LoadSubExtensions(ext);
        LoadSettingsExtension(hostModel, ext);

        return;

        static void LoadSubExtensions(IEnumerable<IExtension> extensions)
        {
            foreach (var extension in extensions)
                extension.OnExtensionLoaded();
        }

        void LoadSettingsExtension(ExtensionsHostModel model, IEnumerable<IExtension> extensions)
        {
            var extensionSettingsGroup = new ExtensionSettingsGroup(model);
            var values = model.Values;
            var settingsExtensions = extensions.OfType<ISettingsExtension>();
            foreach (var settingsExtension in settingsExtensions)
            {
                var token = settingsExtension.Token;
                switch (settingsExtension)
                {
                    case IStringSettingsExtension i:
                    {
                        var value = values.TryGetTargetOrAddDefault(token, i.DefaultValue);
                        extensionSettingsGroup.Add(new ExtensionSettingsEntry<IStringSettingsExtension, string>(i, value, t => t.DefaultValue, i.OnValueChanged));
                        break;
                    }
                    case IIntOrEnumSettingsExtension i:
                    {
                        var value = values.TryGetTargetOrAddDefault(token, i.DefaultValue);
                        switch (i)
                        {
                            case IIntSettingsExtension a:
                                extensionSettingsGroup.Add(new ExtensionIntSettingsEntry(a, value, t => t.DefaultValue, a.OnValueChanged));
                                break;
                            case IEnumSettingsExtension b:
                                extensionSettingsGroup.Add(new ExtensionEnumSettingsEntry(b, value, t => t.DefaultValue, i.OnValueChanged));
                                break;
                        }
                        break;
                    }
                    case IColorSettingsExtension i:
                    {
                        var value = values.TryGetTargetOrAddDefault(token, i.DefaultValue);
                        extensionSettingsGroup.Add(new ExtensionSettingsEntry<IColorSettingsExtension, uint>(i, value, t => t.DefaultValue, i.OnValueChanged));
                        break;
                    }
                    case IStringsArraySettingsExtension i:
                    {
                        var value = values.TryGetTargetOrAddDefault(token, i.DefaultValue);

                        extensionSettingsGroup.Add(new ExtensionSettingsEntry<IStringsArraySettingsExtension, ObservableCollection<string>>(i, [.. value], t => [.. t.DefaultValue], t => i.OnValueChanged([.. t])));
                        break;
                    }
                    case IDateTimeOffsetSettingsExtension i:
                    {
                        var value = values.TryGetTargetOrAddDefault(token, i.DefaultValue);
                        extensionSettingsGroup.Add(new ExtensionSettingsEntry<IDateTimeOffsetSettingsExtension, DateTimeOffset>(i, value, t => t.DefaultValue, i.OnValueChanged));
                        break;
                    }
                    case IBoolSettingsExtension i:
                    {
                        var value = values.TryGetTargetOrAddDefault(token, i.DefaultValue);
                        extensionSettingsGroup.Add(new ExtensionSettingsEntry<IBoolSettingsExtension, bool>(i, value, t => t.DefaultValue, i.OnValueChanged));
                        break;
                    }
                    case IDoubleSettingsExtension i:
                    {
                        var value = values.TryGetTargetOrAddDefault(token, i.DefaultValue);
                        extensionSettingsGroup.Add(new ExtensionDoubleSettingsEntry(i, value, t => t.DefaultValue, i.OnValueChanged));
                        break;
                    }
                    default:
                        break;
                }
            }

            if (extensionSettingsGroup.Count is not 0)
                _settingsGroups.Add(extensionSettingsGroup);
        }
    }

    public bool Disposed { get; private set; }

    public void Dispose()
    {
        if (Disposed)
            return;
        Disposed = true;
        GC.SuppressFinalize(this);
        while (HostModels is [var model, ..])
            UnloadHost(model);
    }
}

