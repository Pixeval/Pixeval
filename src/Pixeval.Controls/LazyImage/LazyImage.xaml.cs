using System.Diagnostics.CodeAnalysis;
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

    [SuppressMessage("Style", "CS0414", Justification = "For {x:Bind}")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local", Justification = "For {x:Bind}")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    private readonly double _progressRingSize = 35;
}
