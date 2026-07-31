// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Global.Enum;
using Misaki;

namespace Pixeval.Models.Filters;

internal static class WorkOrientationFilterEvaluator
{
    public static bool Matches(IArtworkInfo entry, SearchIllustrationRatioPattern filter)
    {
        if (filter is SearchIllustrationRatioPattern.All)
            return true;

        if (entry is not IImageSize image || image.Width <= 0 || image.Height <= 0)
            return false;

        return filter switch
        {
            SearchIllustrationRatioPattern.Landscape => image.Width > image.Height,
            SearchIllustrationRatioPattern.Portrait => image.Height > image.Width,
            SearchIllustrationRatioPattern.Square => image.Width == image.Height,
            _ => false
        };
    }
}
