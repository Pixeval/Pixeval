// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using Avalonia.Controls;

namespace Pixeval.Views.Settings;

public partial class LanguageSettingsValue : ComboBox, IEntryControl<ISingleValueSettingsEntry<string>>
{
    protected override Type StyleKeyOverride => typeof(ComboBox);

    public ISingleValueSettingsEntry<string> Entry { set => DataContext = value; }

    public LanguageSettingsValue() => InitializeComponent();
}
