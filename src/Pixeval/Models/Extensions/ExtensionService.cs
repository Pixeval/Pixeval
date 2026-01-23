// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.AppManagement;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.Extensions.Common.Downloaders;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Extensions.Common.Settings;

namespace Pixeval.Models.Extensions;

public class ExtensionService : IDisposable
{
    public ObservableCollection<ExtensionsHostModel> HostModels { get; } = [];

    public IEnumerable<ExtensionsHostModel> ActiveModels => HostModels.Where(t => t.IsActive);

    public IReadOnlyList<ExtensionSettingsGroup> SettingsGroups => _settingsGroups;

    public IEnumerable<IExtension> Extensions => HostModels.SelectMany(t => t.Extensions);

    public IEnumerable<IExtension> ActiveExtensions => ActiveModels.SelectMany(t => t.Extensions);

    public IEnumerable<IImageTransformerCommandExtension> ActiveImageTransformerCommands => ActiveExtensions.OfType<IImageTransformerCommandExtension>();

    public IEnumerable<ITextTransformerCommandExtension> ActiveTextTransformerCommands => ActiveExtensions.OfType<ITextTransformerCommandExtension>();

    public IEnumerable<IDownloaderExtension> ActiveDownloaders => ActiveExtensions.OfType<IDownloaderExtension>();

    public IEnumerable<IImageFormatProviderExtension> ActiveImageFormatProviders => ActiveExtensions.OfType<IImageFormatProviderExtension>();

    public IEnumerable<INovelFormatProviderExtension> ActiveNovelFormatProviders => ActiveExtensions.OfType<INovelFormatProviderExtension>();

    private readonly List<ExtensionSettingsGroup> _settingsGroups = [];

    public int OutDateExtensionHostsCount { get; private set; }

    public void LoadAllHosts(ILogger logger)
    {
        foreach (var dll in Directory.GetFiles(AppInfo.ExtensionsFolder, "*.dll"))
        {
            _ = TryLoadHost(dll, logger, out var isOutDate);
            if (isOutDate)
                ++OutDateExtensionHostsCount;
        }
    }

    public bool TryLoadHost(string path, ILogger logger, out bool isOutdated)
    {
        isOutdated = false;
        try
        {
            if (LoadHost(path, logger, out isOutdated) is not { } model)
                return false;
            LoadExtensions(model);
            var inserted = false;
            for (var i = HostModels.Count; i > 0; --i)
                if (HostModels[i - 1].Priority < model.Priority)
                {
                    HostModels.Insert(i, model);
                    inserted = true;
                    break;
                }

            if (!inserted)
                HostModels.Insert(0, model);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void UnloadHost(ExtensionsHostModel model)
    {
        _ = HostModels.Remove(model);
        if (_settingsGroups.FirstOrDefault(t => t.Model == model) is { } group)
            _ = _settingsGroups.Remove(group);
        foreach (var extension in model.Extensions)
            extension.OnExtensionUnloaded();
        model.Dispose();
    }

    private static ExtensionsHostModel? LoadHost(string path, ILogger logger, out bool isOutdated)
    {
        isOutdated = false;
        try
        {
            if (!NativeLibrary.TryLoad(path, out var dllHandle))
                return null;
            try
            {
                if (!NativeLibrary.TryGetExport(dllHandle, nameof(IExtensionsHost.DllGetExtensionsHost), out var dllGetExtensionsHostPtr))
                    return null;
                var dllGetExtensionsHost = Marshal.GetDelegateForFunctionPointer<IExtensionsHost.DllGetExtensionsHost>(dllGetExtensionsHostPtr);
                var result = dllGetExtensionsHost(out var ppv);
                if (result is not 0)
                    return null;
                var wrappers = new StrategyBasedComWrappers();
                var rcw = (IExtensionsHost) wrappers.GetOrCreateObjectForComInstance(ppv, CreateObjectFlags.UniqueInstance);
                _ = Marshal.Release(ppv);

                if (rcw.SdkVersion != IExtensionsHost.CurrentSdkVersion.ToString())
                {
                    NativeLibrary.Free(dllHandle);
                    isOutdated = true;
                    return null;
                }
                rcw.Initialize(CultureInfo.CurrentCulture.Name, AppInfo.TempFolder, AppInfo.ExtensionsFolder, logger);
                return new(rcw) { Handle = dllHandle };
            }
            catch
            {
                NativeLibrary.Free(dllHandle);
                return null;
            }
        }
        catch
        {
            return null;
        }
    }

    private void LoadExtensions(ExtensionsHostModel model)
    {
        var extensions = model.Extensions;
        LoadSubExtensions(extensions);
        LoadSettingsExtension(model, extensions);
    }

    private void LoadSubExtensions(IEnumerable<IExtension> extensions)
    {
        foreach (var extension in extensions)
            extension.OnExtensionLoaded();
    }

    private void LoadSettingsExtension(ExtensionsHostModel model, IEnumerable<IExtension> extensions)
    {
        var extensionSettingsGroup = new ExtensionSettingsGroup(model);
        _settingsGroups.Add(extensionSettingsGroup);
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

