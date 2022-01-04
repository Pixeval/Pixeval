#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MakoHelper.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util.Generic;
using Pixeval.Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Util;

public static class MakoHelper
{
    public static IReadOnlyList<int> StickerIds = Enumerates.EnumerableOf(
            Enumerable.Range(301, 10),
            Enumerable.Range(401, 10),
            Enumerable.Range(201, 10),
            Enumerable.Range(101, 10))
        .SelectMany(Functions.Identity<IEnumerable<int>>()).ToList();

    public static IllustrationSortOptionWrapper GetAppSettingDefaultSortOptionWrapper()
    {
        return LocalizedBoxHelper.Of<IllustrationSortOption, IllustrationSortOptionWrapper>(App.AppViewModel.AppSetting.DefaultSortOption);
    }

    public static string? GetThumbnailUrl(this Illustration illustration, ThumbnailUrlOption option)
    {
        return option switch
        {
            ThumbnailUrlOption.Large => illustration.ImageUrls?.Large,
            ThumbnailUrlOption.Medium => illustration.ImageUrls?.Medium,
            ThumbnailUrlOption.SquareMedium => illustration.ImageUrls?.SquareMedium,
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
        };
    }

    public static Uri GenerateIllustrationWebUri(string id)
    {
        return new Uri($"https://www.pixiv.net/artworks/{id}");
    }

    public static Uri GenerateIllustrationPixEzUri(string id)
    {
        return new Uri($"pixez://www.pixiv.net/artworks/{id}");
    }

    public static Uri GenerateIllustrationAppUri(string id)
    {
        return new Uri($"{AppContext.AppProtocol}://illust/{id}");
    }

    public static string? GetOriginalUrl(this Illustration illustration)
    {
        return illustration.ImageUrls?.Original ?? illustration.MetaSinglePage?.OriginalImageUrl;
    }

    public static string GetImageFormat(this Illustration illustration)
    {
        return illustration.GetOriginalUrl() is { } url ? url[url.LastIndexOf(".", StringComparison.Ordinal)..] : string.Empty;
    }

    public static string GetIllustrationThumbnailCacheKey(this Illustration illustration)
    {
        return $"thumbnail-{illustration.GetOriginalUrl() ?? illustration.Id.ToString()}";
    }

    public static string GetIllustrationOriginalImageCacheKey(this Illustration illustration)
    {
        return $"original-{illustration.GetOriginalUrl() ?? illustration.Id.ToString()}";
    }

    public static SortDescription? GetSortDescriptionForIllustration(IllustrationSortOption sortOption)
    {
        return sortOption switch
        {
            IllustrationSortOption.PopularityDescending => new SortDescription(SortDirection.Descending, IllustrationBookmarkComparer.Instance),
            IllustrationSortOption.PublishDateAscending => new SortDescription(SortDirection.Ascending, IllustrationViewModelPublishDateComparer.Instance),
            IllustrationSortOption.PublishDateDescending => new SortDescription(SortDirection.Descending, IllustrationViewModelPublishDateComparer.Instance),
            IllustrationSortOption.DoNotSort => null,
            _ => throw new ArgumentOutOfRangeException(nameof(sortOption), sortOption, null)
        };
    }

    public static bool IsUgoira(this Illustration illustration)
    {
        return illustration.Type!.Equals("ugoira", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsManga(this Illustration illustration)
    {
        return illustration.PageCount > 1;
    }

    public static void Cancel<T>(this IFetchEngine<T> engine)
    {
        engine.EngineHandle.Cancel();
    }

    public static bool IsRestricted(this Illustration illustration)
    {
        return illustration.XRestrict != 0;
    }

    public static XRestrictLevel RestrictLevel(this Illustration illustration)
    {
        return (XRestrictLevel) illustration.XRestrict;
    }

    public static string GenerateStickerDownloadUrl(int id)
    {
        return $"https://s.pximg.net/common/images/stamp/generated-stamps/{id}_s.jpg";
    }

    public static IconSource GetBookmarkButtonIconSource(bool isBookmarked)
    {
        var systemThemeFontFamily = new FontFamily("Segoe MDL2 Assets");
        return isBookmarked
            ? new FontIconSource
            {
                Glyph = "\xEB52", // HeartFill
                Foreground = new SolidColorBrush(Colors.Crimson),
                FontFamily = systemThemeFontFamily
            }
            : new FontIconSource
            {
                Glyph = "\xEB51", // Heart
                FontFamily = systemThemeFontFamily
            };
    }

    public static IconElement GetBookmarkButtonIcon(bool isBookmarked)
    {
        var systemThemeFontFamily = new FontFamily("Segoe MDL2 Assets");
        return isBookmarked
            ? new FontIcon
            {
                Glyph = "\xEB52", // HeartFill
                Foreground = new SolidColorBrush(Colors.Crimson),
                FontFamily = systemThemeFontFamily
            }
            : new FontIcon
            {
                Glyph = "\xEB51", // Heart
                FontFamily = systemThemeFontFamily
            };
    }
}