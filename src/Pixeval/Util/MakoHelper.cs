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

    public static List<BookmarkTag>? IllustrationPrivateBookmarkTags { get; set; }

    public static List<BookmarkTag>? IllustrationPublicBookmarkTags { get; set; }

    public static List<BookmarkTag>? NovelPrivateBookmarkTags { get; set; }

    public static List<BookmarkTag>? NovelPublicBookmarkTags { get; set; }

    public static async Task<List<BookmarkTag>> GetBookmarkTagsAsync(PrivacyPolicy policy, SimpleWorkType type)
    {
        var refreshed = false;
        while (true)
        {
            var result = (policy, type) switch
            {
                (PrivacyPolicy.Private, SimpleWorkType.IllustAndManga) => IllustrationPrivateBookmarkTags,
                (PrivacyPolicy.Public, SimpleWorkType.IllustAndManga) => IllustrationPublicBookmarkTags,
                (PrivacyPolicy.Private, SimpleWorkType.Novel) => NovelPrivateBookmarkTags,
                (PrivacyPolicy.Public, SimpleWorkType.Novel) => NovelPublicBookmarkTags,
                _ => ThrowHelper.ArgumentOutOfRange<(PrivacyPolicy, SimpleWorkType), List<BookmarkTag>>((policy, type))
            };

            if (result is not null)
                return result;

            if (refreshed)
                ThrowHelper.Exception();

            await RefreshBookmarkTagsAsync();
            refreshed = true;
        }
    }

    public static async Task RefreshBookmarkTagsAsync()
    {
        IllustrationPrivateBookmarkTags = await App.AppViewModel.MakoClient.GetBookmarkTagAsync(App.AppViewModel.PixivUid, SimpleWorkType.IllustAndManga, PrivacyPolicy.Private);
        IllustrationPublicBookmarkTags = await App.AppViewModel.MakoClient.GetBookmarkTagAsync(App.AppViewModel.PixivUid, SimpleWorkType.IllustAndManga, PrivacyPolicy.Public);
        NovelPrivateBookmarkTags = await App.AppViewModel.MakoClient.GetBookmarkTagAsync(App.AppViewModel.PixivUid, SimpleWorkType.Novel, PrivacyPolicy.Private);
        NovelPublicBookmarkTags = await App.AppViewModel.MakoClient.GetBookmarkTagAsync(App.AppViewModel.PixivUid, SimpleWorkType.Novel, PrivacyPolicy.Public);
    }

    public static string GetThumbnailUrl(this IWorkEntry workEntry, ThumbnailUrlOption option = ThumbnailUrlOption.Medium)
    {
        return option switch
        {
            ThumbnailUrlOption.Large => workEntry.ThumbnailUrls.Large,
            ThumbnailUrlOption.Medium => workEntry.ThumbnailUrls.Medium,
            ThumbnailUrlOption.SquareMedium => workEntry.ThumbnailUrls.SquareMedium,
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

    public static string GetThumbnailCacheKey(string url) => $"thumbnail-{url}";

    public static string GetOriginalCacheKey(string url) => $"original-{url}";

    public static async ValueTask<string> GetIllustrationOriginalCacheKeyAsync(this IllustrationItemViewModel illustration) => GetOriginalCacheKey(await illustration.GetOriginalSourceUrlAsync());

    public static SortDescription? GetSortDescriptionForIllustration(WorkSortOption sortOption)
    {
        return sortOption switch
        {
            WorkSortOption.PopularityDescending => new(SortDirection.Descending, WorkViewModelBookmarkComparer.Instance),
            WorkSortOption.PublishDateAscending => new(SortDirection.Ascending, WorkViewModelPublishDateComparer.Instance),
            WorkSortOption.PublishDateDescending => new(SortDirection.Descending, WorkViewModelPublishDateComparer.Instance),
            WorkSortOption.DoNotSort => null,
            _ => ThrowHelper.ArgumentOutOfRange<WorkSortOption, SortDescription?>(sortOption)
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

    public static async Task<bool> SetIllustrationBookmarkAsync(long id, bool isBookmarked, bool privately = false, IEnumerable<string>? tags = null)
    {
        var result = await (isBookmarked
            ? App.AppViewModel.MakoClient.PostIllustrationBookmarkAsync(id, privately ? PrivacyPolicy.Private : PrivacyPolicy.Public, tags)
            : App.AppViewModel.MakoClient.RemoveIllustrationBookmarkAsync(id));
        if (result.IsSuccessStatusCode)
        {
            await RefreshBookmarkTagsAsync();
            return isBookmarked;
        }
        return !isBookmarked;
    }

    public static async Task<bool> SetNovelBookmarkAsync(long id, bool isBookmarked, bool privately = false, IEnumerable<string>? tags = null)
    {
        var result = await (isBookmarked
            ? App.AppViewModel.MakoClient.PostNovelBookmarkAsync(id, privately ? PrivacyPolicy.Private : PrivacyPolicy.Public, tags)
            : App.AppViewModel.MakoClient.RemoveNovelBookmarkAsync(id));
        if (result.IsSuccessStatusCode)
        {
            await RefreshBookmarkTagsAsync();
            return isBookmarked;
        }
        return !isBookmarked;
    }
}
