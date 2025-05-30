// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using Windows.Foundation.Collections;

namespace Pixeval.Settings.Models;

public partial class MultiStringsAppSettingsEntry
    : SingleValueSettingsEntry<AppSettings, ObservableCollection<string>>, IMultiStringsAppSettingsEntry
{
    private readonly Func<AppSettings, ObservableCollection<string>> _getter;
    private readonly Action<AppSettings, ObservableCollection<string>> _setter;
    private readonly Func<AppSettings, object> _propertyGetter;

    public MultiStringsAppSettingsEntry(AppSettings settings,
        Expression<Func<AppSettings, object>> property,
        Func<AppSettings, ObservableCollection<string>> getter,
        Action<AppSettings, ObservableCollection<string>> setter) : base(settings, property, getter, setter)
    {
        _getter = getter;
        _setter = setter;
        _propertyGetter = property.Compile();
        Value = getter(Settings);
        Value.CollectionChanged += (_, _) => _setter(Settings, Value);
    }

    public override FrameworkElement Element => new TokenizingSettingsExpander { Entry = this };

    public string? Placeholder { get; set; } = null;

    /// <summary>
    /// 由于<see cref="Value"/>和<see cref="Settings"/>中的值不是同一个引用，所以这里用field缓存
    /// </summary>
    public sealed override ObservableCollection<string> Value
    {
        get;
        set
        {
            if (Equals(Value, value))
                return;
            _setter(Settings, field = value);
            OnPropertyChanged();
            ValueChanged?.Invoke(Value);
        }
    }

    public override void ValueSaving(IPropertySet values)
    {
        if (Converter.TryConvert(Value, out var result))
            values[Token] = result;
    }
}
