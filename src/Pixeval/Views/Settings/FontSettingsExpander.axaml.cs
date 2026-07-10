// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Avalonia.Controls;

namespace Pixeval.Views.Settings;

public partial class FontSettingsExpander : SettingsExpander, IEntryControl<ISingleValueSettingsEntry<ObservableCollection<string>>>
{
    public ISingleValueSettingsEntry<ObservableCollection<string>> Entry
    {
        set
        {
            DataContext = value;
            FontComboBox.SelectedItem = value.Value is [var fontFamilyName, ..] && !string.IsNullOrWhiteSpace(fontFamilyName)
                ? new FontFamily(fontFamilyName)
                : null;
        }
    }

    public FontSettingsExpander() => InitializeComponent();

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is [FontFamily fontFamily])
            FontTokenizingBox.AddToken(fontFamily.Name);
    }
}
