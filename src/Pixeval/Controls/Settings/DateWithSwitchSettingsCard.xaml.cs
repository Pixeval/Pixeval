// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using AutoSettingsPage.Models;
using AutoSettingsPage.WinUI;
using Pixeval.AppManagement;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class DateWithSwitchSettingsCard : IEntryControl<DateWithSwitchSettingsEntry<AppSettings>>
{
    public DateWithSwitchSettingsEntry<AppSettings> Entry { get; set; } = null!;

    private IMinMaxEntry<DateTimeOffset> DateEntry => (IMinMaxEntry<DateTimeOffset>) Entry.Entries[0];

    public DateWithSwitchSettingsCard() => InitializeComponent();
}
