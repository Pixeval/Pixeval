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

    extension(IReadOnlyCollection<IImageFrame> frames)
    {
        public IImageFrame PickClosestFrame(int width, int height) => frames.PickClosest(width, height) ?? Default;
        public IImageFrame PickClosestHeightFrame(int height) => frames.PickClosestHeight(height) ?? Default;
        public IImageFrame PickMaxFrame() => frames.PickMax() ?? Default;
    }

    public static IAnimatedImageFrame PickMaxFrame(this IReadOnlyCollection<IAnimatedImageFrame> frames) => frames.PickMax() ?? DefaultAnimated;
}
