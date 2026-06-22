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
            ComboBox.SelectedItem = new FontFamily(value.Value[0]);
        }
    }

    public FontSettingsExpander() => InitializeComponent();

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is [FontFamily fontFamily])
            TokenizingBox.AddToken(fontFamily.Name);
    }
}
