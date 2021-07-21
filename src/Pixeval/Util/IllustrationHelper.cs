using System;
using Mako.Model;

namespace Pixeval.Util
{
    public enum ThumbnailUrlOptions
    {
        Large, Medium, SquareMedium
    }

    public static class IllustrationHelper
    {
        public static string? GetThumbnailUrl(this Illustration illustration, ThumbnailUrlOptions option)
        {
            return option switch
            {
                ThumbnailUrlOptions.Large        => illustration.ImageUrls?.Large,
                ThumbnailUrlOptions.Medium       => illustration.ImageUrls?.Medium,
                ThumbnailUrlOptions.SquareMedium => illustration.ImageUrls?.SquareMedium,
                _                                => throw new ArgumentOutOfRangeException(nameof(option), option, null)
            };
        }
    }
}