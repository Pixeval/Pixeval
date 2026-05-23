// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoSettingsPage.Models;

namespace Pixeval.Models.Settings.Entries;

public class DateWithSwitchSettingsEntry<TSettings>(
    TSettings settings,
    Expression<Func<TSettings, bool>> property,
    IReadOnlyList<ISettingsEntry> entries)
    : MultiValuesWithSwitchEntry<TSettings>(settings, property, entries)
{
    public IMinMaxEntry<DateTime> DateEntry => (IMinMaxEntry<DateTime>) Entries[0];
}
