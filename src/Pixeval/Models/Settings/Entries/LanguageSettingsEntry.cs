// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq.Expressions;
using AutoSettingsPage.Models;

namespace Pixeval.Models.Settings.Entries;

public class LanguageSettingsEntry<TSettings>(TSettings settings,
Expression<Func<TSettings, string>> property)
    : StringSettingsEntry<TSettings>(settings, property)
{
    /// <inheritdoc />
    public override Uri? DescriptionUri
    {
        get => new Uri("ms-settings:regionlanguage");
        set => throw new NotSupportedException();
    }
}
