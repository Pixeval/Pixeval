using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Pixeval.AppManagement;
using Pixeval.Attributes;
using Pixeval.Controls.Settings;
using WinUI3Utilities;

namespace Pixeval.Settings.Models;

public partial class MultiStringsAppSettingsEntry : SingleValueSettingsEntryBase<ObservableCollection<string>>, IMultiStringsAppSettingsEntry
{
    private readonly Action<AppSettings, ObservableCollection<string>> _setter;
    private readonly IPropertySet _values;
    private readonly Expression<Func<AppSettings, object>> _property;

    public MultiStringsAppSettingsEntry(SettingsPair<AppSettings> settingsPair,
        Expression<Func<AppSettings, object>> property,
        Func<AppSettings, ObservableCollection<string>> getter,
        Action<AppSettings, ObservableCollection<string>> setter) : base("", "", default, settingsPair.Values)
    {
        Settings = settingsPair.Settings;
        _setter = setter;
        _property = property;
        _values = settingsPair.Values;
        Value = getter(Settings);
        // t => (T)t.A
        if (property.Body is not MemberExpression member)
        {
            ThrowHelper.Argument(property);
            return;
        }

        Token = member.Member.Name;
        Attribute = member.Member.GetCustomAttribute<SettingsEntryAttribute>();

        if (Attribute is { } attribute)
        {
            Header = attribute.LocalizedResourceHeader;
            Description = attribute.LocalizedResourceDescription;
            HeaderIcon = attribute.Symbol;
        }
    }

    public override FrameworkElement Element => new TokenizingSettingsExpander { Entry = this };

    public string? Placeholder { get; set; } = null;

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

    public override string Token { get; }

    public SettingsEntryAttribute? Attribute { get; }

    public Action<ObservableCollection<string>>? ValueChanged { get; set; }

    public AppSettings Settings { get; }

    public override void ValueReset()
    {
        OnPropertyChanged(nameof(Value));
        ValueChanged?.Invoke(Value);
    }

    public override void ValueSaving()
    {
        _setter(Settings, Value);
        _values[Token] = Converter.Convert(_property.Compile()(Settings));
    }
}
