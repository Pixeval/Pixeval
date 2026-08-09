// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoSettingsPage.Models;

namespace Pixeval.Models.Settings.Entries;

public class DomainFrontingSettingsEntry<TSettings>(
    TSettings settings,
    Expression<Func<TSettings, bool>> property,
    IReadOnlyList<ISettingsEntry> entries)
    : MultiValuesWithMainValueEntry<TSettings, BoolSettingsEntry<TSettings>>(
        settings,
        new BoolSettingsEntry<TSettings>(settings, property),
        entries);
