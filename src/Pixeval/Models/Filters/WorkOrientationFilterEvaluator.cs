// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Mako.Global.Enum;
using Misaki;

namespace Pixeval.Models.Filters;

internal static class WorkOrientationFilterEvaluator
{
    public static bool Matches(IArtworkInfo entry, SearchIllustrationRatioPattern filter)
    {
        // “外观”表示不限制方向，直接保留所有作品。
        if (filter is SearchIllustrationRatioPattern.All)
            return true;

        // 列表筛选只使用作品提供的主图尺寸；尺寸缺失时无法可靠判断方向。
        if (entry is not IImageSize image || image.Width <= 0 || image.Height <= 0)
            return false;

        // 通过宽高比较区分横屏、竖屏和正方形。
        return filter switch
        {
            SearchIllustrationRatioPattern.Landscape => image.Width > image.Height,
            SearchIllustrationRatioPattern.Portrait => image.Height > image.Width,
            SearchIllustrationRatioPattern.Square => image.Width == image.Height,
            _ => false
        };
    }
}
