using System;
using System.Collections.Generic;
using Mako;
using Misaki;

namespace Pixeval.Util.UI;

public static class FrameSizeHelper
{
    private static IImageFrame Default => new ImageFrame(1, 1)
    {
        ImageUri = new Uri(DefaultImageUrls.ImageNotAvailable)
    };

    public static IImageFrame PickClosestFrame(this IReadOnlyCollection<IImageFrame> frames, int width, int height)
    {
        if (frames.Count is 0)
            return Default;
        IImageFrame closest = null!;
        var closestDiff = int.MaxValue;
        foreach (var frame in frames)
        {
            var xDiff = frame.Width - width;
            var yDiff = frame.Height - height;
            var diff = xDiff * xDiff + yDiff * yDiff;
            if (diff < closestDiff)
            {
                closest = frame;
                closestDiff = diff;
            }
        }
        return closest;
    }

    public static IImageFrame PickClosestHeightFrame(this IReadOnlyCollection<IImageFrame> frames, int height)
    {
        if (frames.Count is 0)
            return Default;
        IImageFrame closest = null!;
        var closestDiff = int.MaxValue;
        foreach (var frame in frames)
        {
            var yDiff = Math.Abs(frame.Height - height);
            if (yDiff < closestDiff)
            {
                closest = frame;
                closestDiff = yDiff;
            }
        }
        return closest;
    }

    public static IImageFrame PickMaxFrame(this IReadOnlyCollection<IImageFrame> frames)
    {
        if (frames.Count is 0)
            return Default;
        IImageFrame max = null!;
        var maxArea = 0;
        foreach (var frame in frames)
        {
            var area = frame.Width * frame.Height;
            if (area > maxArea)
            {
                max = frame;
                maxArea = area;
            }
        }
        return max;
    }
}
