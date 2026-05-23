// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Data.Converters;
using Avalonia.Media;
using Mako.Model;

namespace Pixeval.Views.Entry;

public partial class SpotlightItem : EntryItem
{
    public SpotlightItem() => InitializeComponent();

    public static readonly FuncValueConverter<SpotlightCategory, SolidColorBrush> BackgroundBrushConverter =
        new(category => new(category switch
        {
            SpotlightCategory.Spotlight => Color.FromArgb(0xFF, 0x00, 0x96, 0xFA),
            SpotlightCategory.Tutorial => Color.FromArgb(0xFF, 0x00, 0xD7, 0xA7),
            _ => Color.FromArgb(0xFF, 0xFF, 0x59, 0x00)
        }));
}
