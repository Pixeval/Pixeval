using System;
using Misaki;
using Pixeval.Controls.Windowing;
using Pixeval.Pages.IllustrationViewer;

namespace Pixeval.Util.UI;

public sealed partial class ViewerPageTag(object? parameter = null)
    : NavigationViewTag<IllustrationViewerPage>("", parameter)
{
    public void SetArtwork(IArtworkInfo artwork)
    {
        Header = string.IsNullOrWhiteSpace(artwork.Title) ? artwork.Platform
            switch
            {
                IPlatformInfo.Pixiv => nameof(IPlatformInfo.Pixiv),
                IPlatformInfo.Danbooru => nameof(IPlatformInfo.Danbooru),
                IPlatformInfo.Gelbooru => nameof(IPlatformInfo.Gelbooru),
                IPlatformInfo.Yandere => nameof(IPlatformInfo.Yandere),
                IPlatformInfo.Sankaku => nameof(IPlatformInfo.Sankaku),
                IPlatformInfo.Rule34 => nameof(IPlatformInfo.Rule34),
                _ => ""
            } : artwork.Title;
        ImageUri = GetPlatformUri(artwork.Platform);
    }

    public static Uri GetPlatformUri(string iconName = IPlatformInfo.Pixiv) => new($"ms-appx:///Assets/Images/Platforms/{iconName}.png");
}
