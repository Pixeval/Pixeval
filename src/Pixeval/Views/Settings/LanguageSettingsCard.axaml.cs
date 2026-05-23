// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage.Avalonia;
using CommunityToolkit.Avalonia.Controls;
using Pixeval.AppManagement;
using Pixeval.Models.Settings.Entries;

namespace Pixeval.Views.Settings;

public partial class LanguageSettingsCard : SettingsCard, IEntryControl<LanguageSettingsEntry<AppSettings>>
{
    public LanguageSettingsEntry<AppSettings> Entry { set => DataContext = value; }

    public LanguageSettingsCard() => InitializeComponent();
}
