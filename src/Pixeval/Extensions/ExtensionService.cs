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
using Windows.Foundation.Collections;
using Windows.Win32;
using Microsoft.UI.Xaml;
using Pixeval.Attributes;
using Pixeval.Controls;
using Pixeval.Controls.Settings;
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
            host.Initialize("", AppKnownFolders.Temp.FullPath);
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
                    if (values.TryGetValue(token, out var value) && value is string v)
                    {
                        var strings = converter.ConvertBack<string[]>(v, false)!;
                        i.OnValueChanged(strings);
                    }
                    else 
                    {
                        var str = (string)converter.Convert(i.GetDefaultValue())!;
                        values[token] = v = str;
                    }
                    // extensionSettingsGroup.Add(new ExtensionStringsArraySettingsEntry(i));
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

public partial class ExtensionSettingsGroup(ExtensionsHostModel model) : List<ISettingsEntry>, ISettingsGroup
{
    public string Header { get; } = model.Name;
}

public abstract class ExtensionSettingsEntry<TValue>(ISettingsExtension extension, TValue value, IPropertySet values)
    : ObservableSettingsEntryBase(extension.GetLabel(), extension.GetDescription(), extension.GetIcon()), ISingleValueSettingsEntry<TValue>
{
    public SettingsEntryAttribute? Attribute => null;

    public Action<TValue>? ValueChanged { get; set; }

    public TValue Value
    {
        get;
        set
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value))
                return;
            field = value;
            OnPropertyChanged();
        }
    } = value;

    public override void ValueReset()
    {
    }

    public override void ValueSaving()
    {
        values[extension.GetToken()] = Value;
    }
}

public class ExtensionBoolSettingsEntry(IBoolSettingsExtension extension, bool value, IPropertySet values) : ExtensionSettingsEntry<bool>(extension, value, values)
{
    public override FrameworkElement Element => new BoolSettingsCard { Entry = this };

    public override void ValueSaving()
    {
        extension.OnValueChanged(Value);
        base.ValueSaving();
    }
}

public class ExtensionStringSettingsEntry(IStringSettingsExtension extension, string value, IPropertySet values) : ExtensionSettingsEntry<string>(extension, value, values), IStringSettingsEntry
{
    public override FrameworkElement Element => new StringSettingsCard { Entry = this };

    public string? Placeholder => extension.GetPlaceholder();

    public override void ValueSaving()
    {
        extension.OnValueChanged(Value);
        base.ValueSaving();
    }
}

public class ExtensionIntSettingsEntry : ExtensionSettingsEntry<int>, IDoubleSettingsEntry
{
    private readonly IIntSettingsExtension _extension;

    public ExtensionIntSettingsEntry(IIntSettingsExtension extension, int value, IPropertySet values) : base(extension, value, values)
    {
        _extension = extension;
        ((IDoubleSettingsEntry)this).ValueChanged = v => ValueChanged?.Invoke((int)v);
    }

    public override FrameworkElement Element => new DoubleSettingsCard { Entry = this };

    Action<double>? ISingleValueSettingsEntry<double>.ValueChanged { get; set; }

    double ISingleValueSettingsEntry<double>.Value { get => Value; set => Value = (int)value; }

    public override void ValueSaving()
    {
        _extension.OnValueChanged(Value);
        base.ValueSaving();
    }

    public string? Placeholder => _extension.GetPlaceholder();

    public double Max => _extension.GetMaxValue();

    public double Min => _extension.GetMinValue();

    public double LargeChange => _extension.GetLargeChange();

    public double SmallChange => _extension.GetSmallChange();
}

public class ExtensionDoubleSettingsEntry(IDoubleSettingsExtension extension, double value, IPropertySet values) : ExtensionSettingsEntry<double>(extension, value, values), IDoubleSettingsEntry
{
    public override FrameworkElement Element => new DoubleSettingsCard { Entry = this };

    public override void ValueSaving()
    {
        extension.OnValueChanged(Value);
        base.ValueSaving();
    }

    public string? Placeholder => extension.GetPlaceholder();

    public double Max => extension.GetMaxValue();

    public double Min => extension.GetMinValue();

    public double LargeChange => extension.GetLargeChange();

    public double SmallChange => extension.GetSmallChange();
}

public class ExtensionEnumSettingsEntry(IEnumSettingsExtension extension, int value, IPropertySet values) : ExtensionSettingsEntry<object>(extension, value, values), IEnumSettingsEntry
{
    public override FrameworkElement Element => new EnumSettingsCard { Entry = this };

    public override void ValueSaving()
    {
        extension.OnValueChanged((int)Value);
        base.ValueSaving();
    }

    public IReadOnlyList<StringRepresentableItem> EnumItems => extension.GetEnumKeyValues()
        .Select(t => new StringRepresentableItem(t.Value, t.Key)).ToArray();
}

public class ExtensionColorSettingsEntry(IColorSettingsExtension extension, uint value, IPropertySet values) : ExtensionSettingsEntry<uint>(extension, value, values)
{
    public override FrameworkElement Element => new ColorSettingsCard { Entry = this };

    public override void ValueSaving()
    {
        extension.OnValueChanged(Value);
        base.ValueSaving();
    }
}

public class ExtensionDateSettingsEntry(IDateTimeOffsetSettingsExtension extension, DateTimeOffset value, IPropertySet values) : ExtensionSettingsEntry<DateTimeOffset>(extension, value, values)
{
    public override FrameworkElement Element => new DateSettingsCard { Entry = this };

    public override void ValueSaving()
    {
        extension.OnValueChanged(Value);
        base.ValueSaving();
    }
}

//public class ExtensionStringsArraySettingsEntry(IStringsArraySettingsExtension extension) : ExtensionSettingsEntry<string[]>(extension)
//{
//    public override FrameworkElement Element => new StringSettingsCard { Entry = this };
//    public override string[] Value { get; set; }
//}
