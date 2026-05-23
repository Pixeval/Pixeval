// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage.Avalonia;
using CommunityToolkit.Avalonia.Controls;
using Pixeval.AppManagement;
using Pixeval.Models.Settings.Entries;

namespace Pixeval.Views.Settings;

public partial class DomainFrontingSettingsExpander : SettingsExpander, IEntryControl<DomainFrontingSettingsEntry<AppSettings>>
{
    public DomainFrontingSettingsEntry<AppSettings> Entry
    {
        set
        {
            DataContext = value;
            while (Items.Count > 1)
                Items.RemoveAt(0);
            WrapPanel.Children.Clear();
            foreach (var entry in value.Entries)
                if (entry is IPSetSettingsEntry<AppSettings>)
                    WrapPanel.Children.Add(SettingsEntryHelper.GetControl(entry));
                else
                    Items.Insert(Items.Count - 1, SettingsEntryHelper.GetControl(entry));
        }
    }

    public DomainFrontingSettingsExpander()
    {
        InitializeComponent();
    }
}
