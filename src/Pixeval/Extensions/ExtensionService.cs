// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using Microsoft.Windows.Storage;
using Pixeval.AppManagement;
using Pixeval.Extensions.Common.Settings;
using Pixeval.Extensions.Common;
using System.Linq;
using Windows.Win32;
using Pixeval.Extensions.Models;
using Pixeval.Settings;
using WinUI3Utilities;

namespace Pixeval.Extensions;

public class ExtensionService
{
    public IReadOnlyList<ExtensionsHostModel> ExtensionHosts => _extensionHosts;

    public IReadOnlyDictionary<ExtensionsHostModel, IReadOnlyList<ISettingsExtension>> SettingsExtensions => _settingsExtensions;

    public IReadOnlyList<ISettingsGroup> SettingsGroups => _settingsGroups;

    private readonly List<ExtensionsHostModel> _extensionHosts = [];

    private readonly Dictionary<ExtensionsHostModel, IReadOnlyList<ISettingsExtension>> _settingsExtensions = [];

    private static ApplicationDataContainer LocalSettings => ApplicationData.GetDefault().LocalSettings;

    private readonly List<ISettingsGroup> _settingsGroups = [];

    public void LoadAllExtensions()
    {
        foreach (var dll in AppKnownFolders.Extensions.GetFiles("*.dll"))
        {
            if (LoadExtension(dll) is not { } host)
                continue;
            host.Initialize(AppSettings.CurrentCulture.Name, AppKnownFolders.Temp.FullPath);
            var model = new ExtensionsHostModel(host);
            _extensionHosts.Add(model);
            LoadSettingsForExtension(model);
        }
    }

    private static IExtensionsHost? LoadExtension(string path)
    {
        try
        {
            var dllHandle = PInvoke.LoadLibrary(path);
            if (dllHandle is null)
                return null;
            var dllGetExtensionsHostPtr =
                PInvoke.GetProcAddress(dllHandle, nameof(IExtensionsHost.DllGetExtensionsHost));
            if ((nint)dllGetExtensionsHostPtr is 0)
                return null;
            var dllGetExtensionsHost =
                Marshal.GetDelegateForFunctionPointer<IExtensionsHost.DllGetExtensionsHost>(dllGetExtensionsHostPtr);
            var result = dllGetExtensionsHost(out var ppv);
            if (result is not 0)
                return null;
            var wrappers = new StrategyBasedComWrappers();
            var rcw = (IExtensionsHost)wrappers.GetOrCreateObjectForComInstance(ppv, CreateObjectFlags.UniqueInstance);
            Marshal.Release(ppv);
            return rcw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void LoadSettingsForExtension(ExtensionsHostModel model)
    {
        var settingsExtensions = model.Extensions.OfType<ISettingsExtension>().ToArray();
        _settingsExtensions[model] = settingsExtensions;
        var converter = new SettingsValueConverter();
        if (!LocalSettings.Containers.TryGetValue(model.Name, out var container))
            container = LocalSettings.CreateContainer(model.Name, ApplicationDataCreateDisposition.Always);
        var extensionSettingsGroup = new ExtensionSettingsGroup(model);
        _settingsGroups.Add(extensionSettingsGroup);
        var values = container.Values;
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
                    extensionSettingsGroup.Add(new ExtensionStringSettingsEntry(i, v, values));
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
                            extensionSettingsGroup.Add(new ExtensionIntSettingsEntry(a, v, values));
                            break;
                        case IEnumSettingsExtension b:
                            extensionSettingsGroup.Add(new ExtensionEnumSettingsEntry(b, v, values));
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
                    extensionSettingsGroup.Add(new ExtensionColorSettingsEntry(i, v, values));
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
                    extensionSettingsGroup.Add(new ExtensionStringsArraySettingsEntry(i, v, values));
                    break;
                }
                case IDateTimeOffsetSettingsExtension i:
                {
                    if (values.TryGetValue(token, out var value) && value is DateTimeOffset v)
                        i.OnValueChanged(v);
                    else
                        values[token] = v = i.GetDefaultValue();
                    extensionSettingsGroup.Add(new ExtensionDateSettingsEntry(i, v, values));
                    break;
                }
                case IBoolSettingsExtension i:
                {
                    if (values.TryGetValue(token, out var value) && value is bool v)
                        i.OnValueChanged(v);
                    else
                        values[token] = v = i.GetDefaultValue();
                    extensionSettingsGroup.Add(new ExtensionBoolSettingsEntry(i, v, values));
                    break;
                }
                case IDoubleSettingsExtension i:
                {
                    if (values.TryGetValue(token, out var value) && value is double v)
                        i.OnValueChanged(v);
                    else 
                        values[token] = v = i.GetDefaultValue();
                    extensionSettingsGroup.Add(new ExtensionDoubleSettingsEntry(i, v, values));
                    break;
                }
                default:
                    break;
            }
        }
    }
}
