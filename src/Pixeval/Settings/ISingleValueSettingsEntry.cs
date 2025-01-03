// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using Pixeval.Attributes;
using Pixeval.Controls;

namespace Pixeval.Settings;

public interface ISingleValueSettingsEntry<TValue> : ISettingsEntry
{
    SettingsEntryAttribute? Attribute { get; }

    Action<TValue>? ValueChanged { get; set; }

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

    double SmallChange { get;  }
}

public interface IEnumSettingsEntry : ISingleValueSettingsEntry<object>
{
    public IReadOnlyList<StringRepresentableItem> EnumItems { get; }
}
