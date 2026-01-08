// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Settings.Models;
using AutoSettingsPage.WinUI;

namespace Pixeval.Controls.Settings;

public sealed partial class LanguageSettingsCard : IEntryControl<LanguageAppSettingsEntry>
{
    public LanguageAppSettingsEntry Entry { get; set; } = null!;

    public LanguageSettingsCard() => InitializeComponent();
}
