using System.Collections.Immutable;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Controls.IllustrationView;
using Pixeval.Options;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<IllustrationViewModel>("ViewModel")]
[DependencyProperty<ThumbnailUrlOption>("ThumbnailOption")]
public sealed partial class IllustrationImage : UserControl
{
    public IllustrationImage() => InitializeComponent();

    /// <summary>
    /// 这个方法用来刷新获取缩略图属性
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    private SoftwareBitmapSource? GetThumbnailSource(ImmutableDictionary<ThumbnailUrlOption, SoftwareBitmapSource> dictionary, ThumbnailUrlOption option)
        => dictionary.TryGetValue(option, out var source) ? source : null;
}
