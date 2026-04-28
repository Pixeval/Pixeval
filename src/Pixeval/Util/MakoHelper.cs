// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Collections;
using Pixeval.AppManagement;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.Collections;
using Pixeval.Controls;
using Pixeval.Options;
using static Pixeval.MiscResources;
using WinUI3Utilities;

namespace Pixeval.Util;

public static class MakoHelper
{
    public static readonly IReadOnlyList<int> StickerIds =
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

    public static async Task<List<BookmarkTag>> GetBookmarkTagsAsync(long uid, SimpleWorkType type, PrivacyPolicy policy)
    {
        var tags = await App.AppViewModel.MakoClient.WorkBookmarkTag(uid, type, policy).ToListAsync();
        tags.Insert(0, AllBookmarkTag.Instance);
        return tags;
    }

    public static async Task<List<BookmarkTag>> GetBookmarkTagsAsync(PrivacyPolicy policy, SimpleWorkType type)
    {
        var refreshed = false;
        while (true)
        {
            var result = (policy, type) switch
            {
                (PrivacyPolicy.Private, SimpleWorkType.IllustrationAndManga) => IllustrationPrivateBookmarkTags,
                (PrivacyPolicy.Public, SimpleWorkType.IllustrationAndManga) => IllustrationPublicBookmarkTags,
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
        IllustrationPrivateBookmarkTags = await GetBookmarkTagsAsync(App.AppViewModel.PixivUid, SimpleWorkType.IllustrationAndManga, PrivacyPolicy.Private);
        IllustrationPublicBookmarkTags = await GetBookmarkTagsAsync(App.AppViewModel.PixivUid, SimpleWorkType.IllustrationAndManga, PrivacyPolicy.Public);
        NovelPrivateBookmarkTags = await GetBookmarkTagsAsync(App.AppViewModel.PixivUid, SimpleWorkType.Novel, PrivacyPolicy.Private);
        NovelPublicBookmarkTags = await GetBookmarkTagsAsync(App.AppViewModel.PixivUid, SimpleWorkType.Novel, PrivacyPolicy.Public);
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

    public static Uri GenerateSpotlightWebUri(long id) => new($"https://www.pixivision.net/a/{id}");

    public static Uri GenerateSpotlightPixEzUri(long id) => new($"pixez://www.pixivision.net/a/{id}");

    public static Uri GenerateSpotlightAppUri(long id) => new($"{AppInfo.AppProtocol}://spotlight/{id}");

    public static SortDescription<IWorkViewModel>? GetSortDescription(WorkSortOption sortOption)
    {
        return sortOption switch
        {
            WorkSortOption.PopularityDescending => new(WorkEntryBookmarkComparer.Instance, SortDirection.Descending),
            WorkSortOption.PublishDateAscending => new(WorkEntryPublishDateComparer.Instance, SortDirection.Ascending),
            WorkSortOption.PublishDateDescending => new(WorkEntryPublishDateComparer.Instance, SortDirection.Descending),
            WorkSortOption.DoNotSort => null,
            _ => ThrowHelper.ArgumentOutOfRange<WorkSortOption, SortDescription<IWorkViewModel>?>(sortOption)
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
        return result ? isFollowed : !isFollowed;
    }

    public static async Task<bool> SetIllustrationBookmarkAsync(Illustration id, bool favorite, bool privately = false, IEnumerable<string>? tags = null)
    {
        try
        {
            var result = await (favorite
                ? App.AppViewModel.MakoClient.PostIllustrationBookmarkAsync(id.Id, privately ? PrivacyPolicy.Private : PrivacyPolicy.Public, tags)
                : App.AppViewModel.MakoClient.RemoveIllustrationBookmarkAsync(id.Id));
            if (result)
            {
                await RefreshBookmarkTagsAsync();
                id.IsFavorite = favorite;
            }
            return id.IsFavorite;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static async Task<bool> SetNovelBookmarkAsync(Novel id, bool favorite, bool privately = false, IEnumerable<string>? tags = null)
    {
        var result = await (favorite
            ? App.AppViewModel.MakoClient.PostNovelBookmarkAsync(id.Id, privately ? PrivacyPolicy.Private : PrivacyPolicy.Public, tags)
            : App.AppViewModel.MakoClient.RemoveNovelBookmarkAsync(id.Id));
        if (result)
        {
            await RefreshBookmarkTagsAsync();
            id.IsFavorite = favorite;
        }
        return id.IsFavorite;
    }

    public static async ValueTask TryPreloadListAsync<T>(this IPreloadableList<T> list, IPlatformInfo platform)
    {
        if (!list.IsPreloaded)
            await list.PreloadListAsync(App.AppViewModel.GetRequiredPlatformService<IGetArtworkService>(platform.Platform));
    }
}

public class AllBookmarkTag : BookmarkTag
{
    public static AllBookmarkTag Instance { get; } = new()
    {
        Name = null!,
        Count = 0
    };

    public override string ToString() => AllCountedTagName;
}
