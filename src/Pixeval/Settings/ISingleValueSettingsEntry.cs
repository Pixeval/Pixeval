// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Pixeval.Attributes;
using Pixeval.Controls;

namespace Pixeval.Settings;

public interface ISingleValueSettingsEntry<TValue> : ISettingsEntry, INotifyPropertyChanged
{
    SettingsEntryAttribute? Attribute { get; }

    TValue Value { get; set; }
}

public interface IStringSettingsEntry : ISingleValueSettingsEntry<string>
{
    string? Placeholder { get; }
}

public interface IDoubleSettingsEntry : ISingleValueSettingsEntry<double>
{
    string? Placeholder { get; }

    double Max { get; }

    double Min { get; }

    double LargeChange { get; }

    double SmallChange { get; }
}

public interface IEnumSettingsEntry : ISingleValueSettingsEntry<object>
{
    IReadOnlyList<StringRepresentableItem> EnumItems { get; }
}

public interface IMultiStringsAppSettingsEntry : ISingleValueSettingsEntry<ObservableCollection<string>>
{
    string? Placeholder { get; }
}

public interface IAppSettingEntry<in TSetting> : ISettingsEntry
{
    void ValueReset(TSetting defaultSetting);
}
