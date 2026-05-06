// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.AppManagement;
using Pixeval.Collections;
using Pixeval.I18N;
using Pixeval.Models.Options;
using Pixeval.ViewModels;

namespace Pixeval.Utilities;

public static class MakoHelper
{
    public static async Task<List<BookmarkTag>> GetBookmarkTagsAsync(long uid, SimpleWorkType type, PrivacyPolicy policy)
    {
        var tags = await App.AppViewModel.MakoClient.WorkBookmarkTags(uid, type, policy).ToListAsync();
        tags.Insert(0, AllBookmarkTag.Instance);
        return tags;
    }

    public static string GetThumbnailUrl(this IWorkEntry workEntry, ThumbnailUrlOption option = ThumbnailUrlOption.Medium)
    {
        return option switch
        {
            ThumbnailUrlOption.Large => workEntry.ThumbnailUrls.Large,
            ThumbnailUrlOption.Medium => workEntry.ThumbnailUrls.Medium,
            ThumbnailUrlOption.SquareMedium => workEntry.ThumbnailUrls.SquareMedium,
            _ => throw new ArgumentOutOfRangeException(nameof(option))
        };
    }

    public static Uri GenerateIllustrationWebUri(long id) => new($"https://www.pixiv.net/artworks/{id}");

    public static Uri GenerateUserWebUri(long id) => new($"https://www.pixiv.net/users/{id}");

    public static Uri GenerateSpotlightWebUri(long id) => new($"https://www.pixivision.net/a/{id}");

    public static Uri GenerateSpotlightAppUri(long id) => new($"{AppInfo.AppProtocol}://spotlight/{id}");

    public static IEnumerable<ISortDescription<IWorkViewModel>> GetSortDescription(LocalSortOption sortOption)
    {
        if (sortOption is LocalSortOption.DoNotSort)
            yield break;
        yield return sortOption switch
        {
            LocalSortOption.PopularityDescending => ISortDescription<IWorkViewModel>.Create(t => t.Entry.TotalFavorite, true),
            LocalSortOption.PublishDateAscending => ISortDescription<IWorkViewModel>.Create(t => t.Entry.CreateDate),
            LocalSortOption.PublishDateDescending => ISortDescription<IWorkViewModel>.Create(t => t.Entry.CreateDate, true),
            LocalSortOption.DoNotSort or _ => throw new ArgumentOutOfRangeException(nameof(sortOption))
        };
        yield return ISortDescription<IWorkViewModel>.Create(t => t.Entry.Id);
    }

    public static async Task<bool> SetFollowAsync(User id, bool isFollowed, bool privately = false)
    {
        var result = await (isFollowed
            ? App.AppViewModel.MakoClient.PostFollowUserAsync(id.Id, privately ? PrivacyPolicy.Private : PrivacyPolicy.Public)
            : App.AppViewModel.MakoClient.RemoveFollowUserAsync(id.Id));
        if (result)
            id.UserInfo.IsFollowed = isFollowed;
        return id.UserInfo.IsFollowed;
    }

    public static async Task<bool> SetIllustrationBookmarkAsync(Illustration id, bool favorite, bool privately = false, IEnumerable<string>? tags = null)
    {
        try
        {
            var result = await (favorite
                ? App.AppViewModel.MakoClient.PostIllustrationBookmarkAsync(id.Id, privately ? PrivacyPolicy.Private : PrivacyPolicy.Public, tags)
                : App.AppViewModel.MakoClient.RemoveIllustrationBookmarkAsync(id.Id));
            if (result)
                id.IsFavorite = favorite;
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
            id.IsFavorite = favorite;
        return id.IsFavorite;
    }

    extension<T>(IPreloadableList<T> list)
    {
        public async ValueTask TryPreloadListAsync(IPlatformInfo platform)
        {
            if (!list.IsPreloaded)
                await list.PreloadListAsync(App.AppViewModel.GetRequiredPlatformService<IGetArtworkService>(platform.Platform));
        }

        public async ValueTask TryPreloadListAsync(string platform)
        {
            if (!list.IsPreloaded)
                await list.PreloadListAsync(App.AppViewModel.GetRequiredPlatformService<IGetArtworkService>(platform));
        }
    }

    extension<T>(Task<IReadOnlyList<T>> task)
    {
        public async IAsyncEnumerable<T> ToAsyncEnumerable()
        {
            foreach (var item in await task)
                yield return item;
        }
    }
}

public class AddNewBookmarkTag : BookmarkTag
{
    public required EventHandler<AddNewBookmarkTag, string> TagAdded;
}

public class AllBookmarkTag : BookmarkTag
{
    public static AllBookmarkTag Instance { get; } = new()
    {
        Name = null!,
        Count = 0
    };

    private static readonly string _AllCountedTagName = I18NManager.GetResource(MiscResources.AllCountedTagName);

    /// <inheritdoc />
    public override string ToString() => _AllCountedTagName;
}
