// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Linq;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using Avalonia;
using CommunityToolkit.Avalonia.Controls;
using Pixeval.Models.Settings.Entries;

namespace Pixeval.Views.Settings;

public partial class DomainFrontingSettingsExpander : SettingsExpander, IEntryControl<IMultiValuesWithSwitchSettingsEntry>
{
    public ISettingsEntry? FirstIPSetSettingsEntry
    {
        get;
        private set => SetAndRaise(FirstIPSetSettingsEntryProperty, ref field, value);
    }

    public IMultiValuesWithSwitchSettingsEntry Entry
    {
        set
        {
            DataContext = value;
            FirstIPSetSettingsEntry = value.Entries.FirstOrDefault(static entry => IsIPSetSettingsEntry(entry));
            while (Items.Count > 1)
                Items.RemoveAt(0);
            WrapPanel.Children.Clear();
            foreach (var entry in value.Entries)
                if (IsIPSetSettingsEntry(entry))
                    WrapPanel.Children.Add(SettingsEntryHelper.GetControl(entry));
                else
                    Items.Insert(Items.Count - 1, SettingsEntryHelper.GetControl(entry));
        }
    }

    public DomainFrontingSettingsExpander()
    {
        InitializeComponent();
    }

    private static bool IsIPSetSettingsEntry(ISettingsEntry entry) =>
        entry.GetType().IsGenericType && entry.GetType().GetGenericTypeDefinition() == typeof(IPSetSettingsEntry<>);

    public static readonly DirectProperty<DomainFrontingSettingsExpander, ISettingsEntry?> FirstIPSetSettingsEntryProperty =
        AvaloniaProperty.RegisterDirect<DomainFrontingSettingsExpander, ISettingsEntry?>(
            nameof(FirstIPSetSettingsEntry),
            o => o.FirstIPSetSettingsEntry);
}
