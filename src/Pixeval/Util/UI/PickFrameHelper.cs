using System;
using System.Collections.Generic;
using Mako;
using Misaki;

namespace Pixeval.Util.UI;

public static class FrameSizeHelper
{
    private static IImageFrame Default => new ImageFrame(new ImageSize(1, 1))
    {
        ImageUri = new Uri(DefaultImageUrls.ImageNotAvailable)
    };

    private static IAnimatedImageFrame DefaultAnimated =>
        new AnimatedImageFrame(new ImageSize(1, 1), [(new Uri(DefaultImageUrls.ImageNotAvailable), 1)]);

    public static IImageFrame PickClosestFrame(this IReadOnlyCollection<IImageFrame> frames, int width, int height) => frames.PickClosest(width, height) ?? Default;

    public static IImageFrame PickClosestHeightFrame(this IReadOnlyCollection<IImageFrame> frames, int height) => frames.PickClosestHeight(height) ?? Default;

    public static IImageFrame PickMaxFrame(this IReadOnlyCollection<IImageFrame> frames) => frames.PickMax() ?? Default;

    public static IAnimatedImageFrame PickMaxFrame(this IReadOnlyCollection<IAnimatedImageFrame> frames) => frames.PickMax() ?? DefaultAnimated;
}
