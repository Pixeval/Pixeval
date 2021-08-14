using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI.UI;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Options;
using Pixeval.ViewModel;

namespace Pixeval.Util
{
    public static class MakoHelper
    {
        public static IllustrationSortOptionWrapper GetAppSettingDefaultSortOptionWrapper()
        {
            return IllustrationSortOptionWrapper.Available.First(it => it.Value == App.AppSetting.DefaultSortOption);
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
            return new($"https://www.pixiv.net/artworks/{id}");
        }

        public static string? GetOriginalUrl(this Illustration illustration)
        {
            return illustration.ImageUrls?.Original ?? illustration.MetaSinglePage?.OriginalImageUrl;
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

        public static int Compare<K>(this IllustrationViewModel? m1, IllustrationViewModel? m2, Func<IllustrationViewModel, K> keySelector)
            where K : IComparable<K>
        {
            if (m1 is null || m2 is null)
            {
                return 0;
            }

            var key1 = keySelector(m1);
            var key2 = keySelector(m2);
            return key1.CompareTo(key2);
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