using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<ImageSource>("Source", DependencyPropertyDefaultValue.UnsetValue, nameof(OnSourceChanged), IsNullable = true)]
[INotifyPropertyChanged]
public sealed partial class LazyImage : UserControl
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
