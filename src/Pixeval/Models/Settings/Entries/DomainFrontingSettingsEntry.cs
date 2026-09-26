// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoSettingsPage.Models;

namespace Pixeval.Models.Settings.Entries;

public class DomainFrontingSettingsEntry<TSettings, TSubSettings>(
    TSubSettings settings,
    Expression<Func<TSettings, TSubSettings>> subSettingsProperty,
    Expression<Func<TSubSettings, bool>> mainValueProperty,
    IReadOnlyList<ISettingsEntry> entries)
    : MultiValuesWithMainValueEntry<TSettings, TSubSettings, BoolSettingsEntry<TSubSettings>>(
        settings,
        subSettingsProperty,
        new BoolSettingsEntry<TSubSettings>(settings, mainValueProperty),
        entries);
