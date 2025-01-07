// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<ImageSource>("Source", DependencyPropertyDefaultValue.UnsetValue, nameof(OnSourceChanged), IsNullable = true)]
[DependencyProperty<Stretch>("Stretch", "Microsoft.UI.Xaml.Media.Stretch.UniformToFill")]
[DependencyProperty<HorizontalAlignment>("HorizontalImageAlignment", "Microsoft.UI.Xaml.HorizontalAlignment.Center")]
[DependencyProperty<VerticalAlignment>("VerticalImageAlignment", "Microsoft.UI.Xaml.VerticalAlignment.Center")]
[INotifyPropertyChanged]
public sealed partial class LazyImage
{
    public LazyImage() => InitializeComponent();

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.To<LazyImage>().OnPropertyChanged(nameof(ShowProgressRing));
    }

    private bool ShowProgressRing => Source is null;

    /// <summary>
    /// ReSharper disable once ConvertToConstant.Local
    /// </summary>
#pragma warning disable CS0414
    private readonly double _progressRingSize = 35;
#pragma warning restore CS0414
}
