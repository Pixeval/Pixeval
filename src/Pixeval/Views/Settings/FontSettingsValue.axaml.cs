// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Avalonia.Controls.Converters;
using Pixeval.Utilities;

namespace Pixeval.Views.Settings;

public partial class FontSettingsValue : UserControl, IEntryControl<ISingleValueSettingsEntry<ObservableCollection<string>>>
{
    public static DoubleLessThanOrEqualConverter NarrowLayoutConverter { get; } = new() { Threshold = 476 };

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

    public FontSettingsValue() => InitializeComponent();

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is [FontFamily fontFamily])
            FontTokenizingBox.AddToken(fontFamily.Name);
    }
}
