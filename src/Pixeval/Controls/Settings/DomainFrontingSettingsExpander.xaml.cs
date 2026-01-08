// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using AutoSettingsPage.Models;
using AutoSettingsPage.WinUI;
using Pixeval.AppManagement;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class DomainFrontingSettingsExpander : IEntryControl<DomainFrontingSettingsEntry<AppSettings>>
{
    public DomainFrontingSettingsEntry<AppSettings> Entry
    {
        get;
        set
        {
            field = value;
            WrapPanel.Children.Clear();
            foreach (var entry in Entry.Entries)
                if (entry is IPSetSettingsEntry<AppSettings>)
                    WrapPanel.Children.Add(SettingsEntryHelper.GetControl(entry));
        }
    } = null!;

    public ISettingsEntry FirstEntry => Entry.Entries[0];

    public DomainFrontingSettingsExpander() => InitializeComponent();
}
