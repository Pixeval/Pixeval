// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using Pixeval.AppManagement;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Extensions.Common;
using System.Linq;
using Windows.Win32;
using Pixeval.Extensions.Common.Transformers;
using Pixeval.Extensions.Models;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Extensions;

public partial class ExtensionService : IDisposable
{
    public IReadOnlyList<ExtensionsHostModel> HostModels => _hostModels;

    public IReadOnlyDictionary<ExtensionsHostModel, IReadOnlyList<IExtension>> ExtensionsLookup => _extensionsLookup;

    public IEnumerable<KeyValuePair<ExtensionsHostModel, IReadOnlyList<IExtension>>> ActivePairs => ExtensionsLookup.Where(t => t.Key.IsActive);

    public IReadOnlyList<ExtensionSettingsGroup> SettingsGroups => _settingsGroups;

    public IReadOnlyList<IExtension> Extensions => ExtensionsLookup
        .Aggregate(new List<IExtension>(), (o, t) => o.Apply(p => p.AddRange(t.Value)));

    public IReadOnlyList<IExtension> ActiveExtensions => ActivePairs
        .Aggregate(new List<IExtension>(), (o, t) => o.Apply(p => p.AddRange(t.Value)));

    public IEnumerable<IImageTransformerExtension> ActiveImageTransformers => ActiveExtensions.OfType<IImageTransformerExtension>();

    private readonly ObservableCollection<ExtensionsHostModel> _hostModels = [];

    private readonly Dictionary<ExtensionsHostModel, IReadOnlyList<IExtension>> _extensionsLookup = [];

    private readonly List<ExtensionSettingsGroup> _settingsGroups = [];

    public void LoadAllHosts()
    {
        foreach (var dll in AppKnownFolders.Extensions.GetFiles("*.dll")) 
            _ = TryLoadHost(dll);
    }

    public bool TryLoadHost(string path)
    {
        try
        {
            if (LoadHost(path) is not { } model)
                return false;
            model.Host.Initialize(AppSettings.CurrentCulture.Name, AppKnownFolders.Temp.FullPath, AppKnownFolders.Extensions.FullPath);
            _hostModels.Add(model);
            LoadExtensions(model);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void UnloadHost(ExtensionsHostModel model)
    {
        _ = _hostModels.Remove(model);
        _ = _extensionsLookup.Remove(model);
        if (_settingsGroups.FirstOrDefault(t => t.Model == model) is { } group)
            _ = _settingsGroups.Remove(group);
        foreach (var extension in model.Extensions)
            extension.OnExtensionUnloaded();
        model.Dispose();
    }

    private static ExtensionsHostModel? LoadHost(string path)
    {
        try
        {
            var dllHandle = PInvoke.LoadLibrary(path);
            if (dllHandle is null)
                return null;
            try
            {
                var dllGetExtensionsHostPtr =
                    PInvoke.GetProcAddress(dllHandle, nameof(IExtensionsHost.DllGetExtensionsHost));
                if ((nint)dllGetExtensionsHostPtr is 0)
                    return null;
                var dllGetExtensionsHost = Marshal.GetDelegateForFunctionPointer<IExtensionsHost.DllGetExtensionsHost>(dllGetExtensionsHostPtr);
                var result = dllGetExtensionsHost(out var ppv);
                if (result is not 0)
                    return null;
                var wrappers = new StrategyBasedComWrappers();
                var rcw = (IExtensionsHost)wrappers.GetOrCreateObjectForComInstance(ppv, CreateObjectFlags.UniqueInstance);
                Marshal.Release(ppv);
                return new(rcw) { Handle = dllHandle };
            }
            catch
            {
                dllHandle.Dispose();
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
        var extensions = model.Extensions.ToArray();
        _extensionsLookup[model] = extensions;
        foreach (var extension in extensions)
            extension.OnExtensionLoaded();
        LoadSettingsExtension(model, extensions);
        LoadImageTransformerExtensions(model, extensions);
    }

    private void LoadSettingsExtension(ExtensionsHostModel model, IEnumerable<IExtension> extensions)
    {
        var converter = new SettingsValueConverter();
        var extensionSettingsGroup = new ExtensionSettingsGroup(model);
        _settingsGroups.Add(extensionSettingsGroup);
        var values = model.Values;
        var settingsExtensions = extensions.OfType<ISettingsExtension>();
        foreach (var settingsExtension in settingsExtensions)
        {
            settingsExtension.OnExtensionLoaded();
            var token = settingsExtension.GetToken();
            switch (settingsExtension)
            {
                case IStringSettingsExtension i:
                {
                    if (values.TryGetValue(token, out var value) && value is string v)
                        i.OnValueChanged(v);
                    else
                       values[token] = v = i.GetDefaultValue();
                    extensionSettingsGroup.Add(new ExtensionStringSettingsEntry(i, v));
                    break;
                }
                case IIntOrEnumSettingsExtension i:
                {
                    if (values.TryGetValue(token, out var value) && value is int v)
                        i.OnValueChanged(v);
                    else 
                        values[token] = v = i.GetDefaultValue();
                    switch (i)
                    {
                        case IIntSettingsExtension a:
                            extensionSettingsGroup.Add(new ExtensionIntSettingsEntry(a, v));
                            break;
                        case IEnumSettingsExtension b:
                            extensionSettingsGroup.Add(new ExtensionEnumSettingsEntry(b, v));
                            break;
                    }
                    break;
                }
                case IColorSettingsExtension i:
                {
                    if (values.TryGetValue(token, out var value) && value is uint v)
                        i.OnValueChanged(v);
                    else 
                        values[token] = v = i.GetDefaultValue();
                    extensionSettingsGroup.Add(new ExtensionColorSettingsEntry(i, v));
                    break;
                }
                case IStringsArraySettingsExtension i:
                {
                    string[] v;
                    if (values.TryGetValue(token, out var value) && value is string s)
                    {
                        v = converter.ConvertBack<string[]>(s, false)!;
                        i.OnValueChanged(v);
                    }
                    else
                    {
                        v = i.GetDefaultValue();
                        values[token] = converter.Convert(v);
                    }
                    extensionSettingsGroup.Add(new ExtensionStringsArraySettingsEntry(i, v));
                    break;
                }
                case IDateTimeOffsetSettingsExtension i:
                {
                    if (values.TryGetValue(token, out var value) && value is DateTimeOffset v)
                        i.OnValueChanged(v);
                    else
                        values[token] = v = i.GetDefaultValue();
                    extensionSettingsGroup.Add(new ExtensionDateSettingsEntry(i, v));
                    break;
                }
                case IBoolSettingsExtension i:
                {
                    if (values.TryGetValue(token, out var value) && value is bool v)
                        i.OnValueChanged(v);
                    else
                        values[token] = v = i.GetDefaultValue();
                    extensionSettingsGroup.Add(new ExtensionBoolSettingsEntry(i, v));
                    break;
                }
                case IDoubleSettingsExtension i:
                {
                    if (values.TryGetValue(token, out var value) && value is double v)
                        i.OnValueChanged(v);
                    else 
                        values[token] = v = i.GetDefaultValue();
                    extensionSettingsGroup.Add(new ExtensionDoubleSettingsEntry(i, v));
                    break;
                }
                default:
                    break;
            }
        }
    }

    private void LoadImageTransformerExtensions(ExtensionsHostModel model, IEnumerable<IExtension> extensions)
    {
        var imageTransformers = extensions.OfType<IImageTransformerExtension>();
        foreach (var imageTransformer in imageTransformers)
            imageTransformer.OnExtensionLoaded();
    }

    public void Dispose()
    {
        while (HostModels is [{ } model, ..])
            UnloadHost(model);
        GC.SuppressFinalize(this);
    }
}
