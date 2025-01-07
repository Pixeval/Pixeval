// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Model;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<SpotlightItemViewModel>("ViewModel", propertyChanged: nameof(OnViewModelChanged))]
public sealed partial class SpotlightItem
{
    public event Action<SpotlightItem, SpotlightItemViewModel>? ViewModelChanged;

    public SpotlightItem() => InitializeComponent();

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d as SpotlightItem is { } item)
        {
            item.ViewModelChanged?.Invoke(item, item.ViewModel);
        }
    }

    private SolidColorBrush GetBackgroundBrush(SpotlightCategory category)
    {
        return new SolidColorBrush(category switch
        {
            SpotlightCategory.Spotlight => Color.FromArgb(0xFF, 0x00, 0x96, 0xFA),
            SpotlightCategory.Tutorial => Color.FromArgb(0xFF, 0x00, 0xD7, 0xA7),
            _ => Color.FromArgb(0xFF, 0xFF, 0x59, 0x00)
        });
    }
}
