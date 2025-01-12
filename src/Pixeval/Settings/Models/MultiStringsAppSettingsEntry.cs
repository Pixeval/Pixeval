// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;

namespace Pixeval.Settings.Models;

public partial class MultiStringsAppSettingsEntry 
    : SingleValueSettingsEntry<AppSettings,ObservableCollection<string>>, IMultiStringsAppSettingsEntry
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
        // 不用Value而用Settings中本身的值，更符合逻辑
        values[Token] = Converter.Convert(_propertyGetter(Settings));
    }
}
