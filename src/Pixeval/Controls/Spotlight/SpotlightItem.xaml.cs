// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Mako.Model;
using Windows.UI;

namespace Pixeval.Controls;

public sealed partial class SpotlightItem
{
    [GeneratedDependencyProperty]
    public partial SpotlightItemViewModel ViewModel { get; set; }

    public event Action<SpotlightItem, SpotlightItemViewModel>? ViewModelChanged;

    partial void OnViewModelPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        ViewModelChanged?.Invoke(this, ViewModel);
    }

    public SpotlightItem() => InitializeComponent();

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
