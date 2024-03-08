#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/MakoHelper.cs
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
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Collections;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;
using Pixeval.Options;
using WinUI3Utilities;

namespace Pixeval.Util;

public static class MakoHelper
{
    public static IReadOnlyList<int> StickerIds =
    [
        .. Enumerable.Range(301, 10),
        .. Enumerable.Range(401, 10),
        .. Enumerable.Range(201, 10),
        .. Enumerable.Range(101, 10)
    ];

    public static string GetThumbnailUrl(this Illustration illustration, ThumbnailUrlOption option = ThumbnailUrlOption.Medium)
    {
        return option switch
        {
            ThumbnailUrlOption.Large => illustration.ImageUrls.Large,
            ThumbnailUrlOption.Medium => illustration.ImageUrls.Medium,
            ThumbnailUrlOption.SquareMedium => illustration.ImageUrls.SquareMedium,
            _ => ThrowHelper.ArgumentOutOfRange<ThumbnailUrlOption, string?>(option)
        };
    }

    public static Uri GenerateIllustrationWebUri(long id) => new($"https://www.pixiv.net/artworks/{id}");

    public static Uri GenerateIllustrationPixEzUri(long id) => new($"pixez://www.pixiv.net/artworks/{id}");

    public static Uri GenerateIllustrationAppUri(long id) => new($"{AppInfo.AppProtocol}://illust/{id}");

    public static Uri GenerateUserWebUri(long id) => new($"https://www.pixiv.net/users/{id}");

    public static Uri GenerateUserPixEzUri(long id) => new($"pixez://www.pixiv.net/users/{id}");

    public static Uri GenerateUserAppUri(long id) => new($"{AppInfo.AppProtocol}://user/{id}");

    public static Uri GenerateNovelWebUri(long id) => new($"https://www.pixiv.net/novel/show.php?id={id}");

    public static Uri GenerateNovelPixEzUri(long id) => new($"pixez://www.pixiv.net/novel/show.php?id={id}");

    public static Uri GenerateNovelAppUri(long id) => new($"{AppInfo.AppProtocol}://novel/{id}");

    public static string GetCacheKeyForThumbnailAsync(string url, ThumbnailUrlOption thumbnailUrlOption = ThumbnailUrlOption.Medium)
    {
        return $"thumbnail-{thumbnailUrlOption}-{url}";
    }

    public static async Task<string> GetIllustrationThumbnailCacheKeyAsync(this IllustrationItemViewModel illustration, ThumbnailUrlOption thumbnailUrlOption = ThumbnailUrlOption.Medium)
    {
        return $"thumbnail-{thumbnailUrlOption}-{await illustration.GetOriginalSourceUrlAsync()}";
    }

    public static async Task<string> GetIllustrationOriginalImageCacheKeyAsync(this IllustrationItemViewModel illustration)
    {
        return $"original-{await illustration.GetOriginalSourceUrlAsync()}";
    }

    public static SortDescription? GetSortDescriptionForIllustration(IllustrationSortOption sortOption)
    {
        return sortOption switch
        {
            IllustrationSortOption.PopularityDescending => new(SortDirection.Descending, IllustrationBookmarkComparer.Instance),
            IllustrationSortOption.PublishDateAscending => new(SortDirection.Ascending, IllustrationViewModelPublishDateComparer.Instance),
            IllustrationSortOption.PublishDateDescending => new(SortDirection.Descending, IllustrationViewModelPublishDateComparer.Instance),
            IllustrationSortOption.DoNotSort => null,
            _ => ThrowHelper.ArgumentOutOfRange<IllustrationSortOption, SortDescription?>(sortOption)
        };
    }

    public static string GenerateStickerDownloadUrl(int id)
    {
        return $"https://s.pximg.net/common/images/stamp/generated-stamps/{id}_s.jpg";
    }

    public static async Task<bool> SetFollowAsync(long id, bool isFollowed, bool privately = false)
    {
        var result = await (isFollowed
            ? App.AppViewModel.MakoClient.PostFollowUserAsync(id, privately ? PrivacyPolicy.Private : PrivacyPolicy.Public)
            : App.AppViewModel.MakoClient.RemoveFollowUserAsync(id));
        return result.IsSuccessStatusCode ? isFollowed : !isFollowed;
    }

    public static async Task<bool> SetIllustrationBookmarkAsync(long id, bool isBookmarked, bool privately = false)
    {
        var result = await (isBookmarked
            ? App.AppViewModel.MakoClient.PostIllustrationBookmarkAsync(id, privately ? PrivacyPolicy.Private : PrivacyPolicy.Public)
            : App.AppViewModel.MakoClient.RemoveIllustrationBookmarkAsync(id));
        return result.IsSuccessStatusCode ? isBookmarked : !isBookmarked;
    }

    public static async Task<bool> SetNovelBookmarkAsync(long id, bool isBookmarked, bool privately = false)
    {
        var result = await (isBookmarked
            ? App.AppViewModel.MakoClient.PostNovelBookmarkAsync(id, privately ? PrivacyPolicy.Private : PrivacyPolicy.Public)
            : App.AppViewModel.MakoClient.RemoveNovelBookmarkAsync(id));
        return result.IsSuccessStatusCode ? isBookmarked : !isBookmarked;
    }
}
