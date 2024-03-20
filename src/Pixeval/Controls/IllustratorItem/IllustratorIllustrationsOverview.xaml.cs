using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<IllustratorIllustrationsOverviewViewModel>("ViewModel")]
public sealed partial class IllustratorIllustrationsOverview
{
    public IllustratorIllustrationsOverview() => InitializeComponent();

    private Thickness GetIllustrationAt(List<SoftwareBitmapSource> sources)
    {
        BannerContainer.Children.Clear();
        var i = 0;
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var source in sources)
        {
            var image = new Image
            {
                Source = source,
                Stretch = Stretch.UniformToFill
            };
            Grid.SetColumn(image, i);
            BannerContainer.Children.Add(image);
            ++i;
        }

        return new Thickness();
    }
}
