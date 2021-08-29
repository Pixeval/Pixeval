using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI.UI;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Options;
using Pixeval.Util.Generic;

namespace Pixeval.Util
{
    public static class MakoHelper
    {
        public static IllustrationSortOptionWrapper GetAppSettingDefaultSortOptionWrapper()
        {
            return LocalizedBoxHelper.Of<IllustrationSortOption, IllustrationSortOptionWrapper>(App.AppSetting.DefaultSortOption);
        }

        public static string? GetThumbnailUrl(this Illustration illustration, ThumbnailUrlOption option)
        {
            return option switch
            {
                ThumbnailUrlOption.Large        => illustration.ImageUrls?.Large,
                ThumbnailUrlOption.Medium       => illustration.ImageUrls?.Medium,
                ThumbnailUrlOption.SquareMedium => illustration.ImageUrls?.SquareMedium,
                _                                => throw new ArgumentOutOfRangeException(nameof(option), option, null)
            };
        }

        public static Uri GetIllustrationWebUri(string id)
        {
            return new Uri($"https://www.pixiv.net/artworks/{id}");
        }

        public static string? GetOriginalUrl(this Illustration illustration)
        {
            return illustration.ImageUrls?.Original ?? illustration.MetaSinglePage?.OriginalImageUrl;
        }

        public static string GetIllustrationCacheKey(this Illustration illustration)
        {
            return illustration.GetOriginalUrl() ?? illustration.Id.ToString();
        }

        public static IEnumerable<Illustration> GetMangaIllustrations(this Illustration illust)
        {
            if (illust.PageCount <= 1)
            {
                return new[] {illust};
            }

            return illust.MetaPages!.Select(m => illust with
            {
                ImageUrls = m.ImageUrls
            });
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
    }
}