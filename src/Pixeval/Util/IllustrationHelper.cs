using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.ViewModel;

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

        public static string GetWebUrl(string id)
        {
            return $"https://www.pixiv.net/artworks/{id}";
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

        public static Action<IList<IllustrationViewModel>, IllustrationViewModel?> Insert(IllustrationSortOption sortOption)
        {
            return sortOption switch
            {
                IllustrationSortOption.PublishDateAscending => (models, model) => models!.AddSorted(model, (m1, m2) => m1.Compare(m2, m => m.PublishDate)),
                IllustrationSortOption.PublishDateDescending => (models, model) => models!.AddSorted(model, (m1, m2) => -m1.Compare(m2, m => m.PublishDate)),
                IllustrationSortOption.PopularityDescending => (models, model) => models!.AddSorted(model, (m1, m2) => -m1.Compare(m2, m => m.Bookmark)),
                _ => throw new ArgumentOutOfRangeException(nameof(sortOption), sortOption, null)
            };
        }
    }
}