// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Linq;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using Avalonia;
using CommunityToolkit.Avalonia.Controls;
using Pixeval.Models.Settings.Entries;

namespace Pixeval.Views.Settings;

public partial class DomainFrontingSettingsExpander : SettingsExpander, IEntryControl<IMultiValuesWithMainValueSettingsEntry<ISingleValueSettingsEntry<bool>>>
{
    public ISettingsEntry? FirstIPSetSettingsEntry
    {
        get;
        private set => SetAndRaise(FirstIPSetSettingsEntryProperty, ref field, value);
    }

    public IMultiValuesWithMainValueSettingsEntry<ISingleValueSettingsEntry<bool>> Entry
    {
        set
        {
            DataContext = value;
            FirstIPSetSettingsEntry = value.Entries.FirstOrDefault(static entry => entry is IIPSetSettingsEntry);
            while (Items.Count > 1)
                Items.RemoveAt(0);
            WrapPanel.Children.Clear();
            foreach (var entry in value.Entries)
            {
                if (entry is IIPSetSettingsEntry valueEntry)
                    WrapPanel.Children.Add(SettingsEntryHelper.GetValueControl(valueEntry, value));
                else
                    Items.Insert(Items.Count - 1, SettingsEntryHelper.GetControl(entry));
            }
        }
    }

    public DomainFrontingSettingsExpander() => InitializeComponent();

    public static readonly DirectProperty<DomainFrontingSettingsExpander, ISettingsEntry?> FirstIPSetSettingsEntryProperty =
        AvaloniaProperty.RegisterDirect<DomainFrontingSettingsExpander, ISettingsEntry?>(
            nameof(FirstIPSetSettingsEntry),
            o => o.FirstIPSetSettingsEntry);
}
