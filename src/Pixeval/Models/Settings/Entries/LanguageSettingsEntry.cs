// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq.Expressions;
using AutoSettingsPage.Models;

namespace Pixeval.Models.Settings.Entries;

public class LanguageSettingsEntry<TSettings>(
    TSettings settings,
    Expression<Func<TSettings, string>> property)
    : StringSettingsEntry<TSettings>(settings, property);
