// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Controls;

public sealed partial class LazyImage
{
    [GeneratedDependencyProperty]
    public partial ImageSource? Source { get; set; }

    [GeneratedDependencyProperty(DefaultValue = Stretch.UniformToFill)]
    public partial Stretch Stretch { get; set; }

    [GeneratedDependencyProperty(DefaultValue = HorizontalAlignment.Center)]
    public partial HorizontalAlignment HorizontalImageAlignment { get; set; }

    [GeneratedDependencyProperty(DefaultValue = VerticalAlignment.Center)]
    public partial VerticalAlignment VerticalImageAlignment { get; set; }

    public LazyImage() => InitializeComponent();

    /// <summary>
    /// ReSharper disable once ConvertToConstant.Local
    /// </summary>
#pragma warning disable CS0414
    private readonly double _progressRingSize = 35;
#pragma warning restore CS0414
}
