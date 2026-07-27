// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Avalonia.Controls;
using Pixeval.Utilities;

namespace Pixeval.Views.Settings;

public partial class FontSettingsExpander : SettingsExpander, IEntryControl<ISingleValueSettingsEntry<ObservableCollection<string>>>
{
    public static IReadOnlyList<FontFamily> AvailableFonts { get; } =
        [.. FontManager.Current.SystemFonts.Where(FontFamilyHelper.IsUsable)];

    public ISingleValueSettingsEntry<ObservableCollection<string>> Entry
    {
        set
        {
            DataContext = value;
            FontComboBox.SelectedItem = value.Value is [var fontFamilyName, ..] && !string.IsNullOrWhiteSpace(fontFamilyName)
                ? FontFamilyHelper.Create(fontFamilyName)
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
