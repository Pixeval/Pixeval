// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using CommunityToolkit.Avalonia.Controls;

namespace Pixeval.Views.Settings;

public partial class LanguageSettingsCard : SettingsCard, IEntryControl<ISingleValueSettingsEntry<string>>
{
    public ISingleValueSettingsEntry<string> Entry { set => DataContext = value; }

    public LanguageSettingsCard() => InitializeComponent();
}
