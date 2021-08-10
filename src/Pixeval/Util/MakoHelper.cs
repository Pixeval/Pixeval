using System;
using System.Collections.Generic;
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

    public record IllustrationSortOptionWrapper : ILocalizedBox<IllustrationSortOption>
    {
        public IllustrationSortOption Value { get; }

        public string LocalizedString { get; }

        public IllustrationSortOptionWrapper(IllustrationSortOption value, string localizedString)
        {
            Value = value;
            LocalizedString = localizedString;
        }
    }

    public record SearchTagMatchOptionWrapper : ILocalizedBox<SearchTagMatchOption>
    {
        public SearchTagMatchOption Value { get; }

        public string LocalizedString { get; }

        public SearchTagMatchOptionWrapper(SearchTagMatchOption value, string localizedString)
        {
            Value = value;
            LocalizedString = localizedString;
        }
    }

    public record TargetFilterWrapper : ILocalizedBox<TargetFilter>
    {
        public TargetFilter Value { get; }

        public string LocalizedString { get; }

        public TargetFilterWrapper(TargetFilter value, string localizedString)
        {
            Value = value;
            LocalizedString = localizedString;
        }
    }

    public static class MakoHelper
    {
        /// <summary>
        /// The <see cref="IllustrationSortOption"/> is welded into Mako, we cannot add <see cref="LocalizedResource"/> on its fields
        /// so an alternative way is chosen
        /// And we cannot use generic since the xaml compiler will refuse to compile
        /// </summary>
        public static readonly IEnumerable<IllustrationSortOptionWrapper> AvailableIllustrationSortOptions = new IllustrationSortOptionWrapper[]
        {
            new(IllustrationSortOption.PublishDateDescending, MiscResources.IllustrationSortOptionPublishDateDescending),
            new(IllustrationSortOption.PublishDateAscending, MiscResources.IllustrationSortOptionPublishDateAscending),
            new(IllustrationSortOption.PopularityDescending, MiscResources.IllustrationSortOptionPopularityDescending),
            new(IllustrationSortOption.DoNotSort, MiscResources.IllustrationSortOptionDoNotSort)
        };

        /// <summary>
        /// The same reason as <see cref="AvailableIllustrationSortOptions"/>
        /// </summary>
        public static readonly IEnumerable<SearchTagMatchOptionWrapper> AvailableSearchTagMatchOption = new SearchTagMatchOptionWrapper[]
        {
            new(SearchTagMatchOption.PartialMatchForTags, MiscResources.SearchTagMatchOptionPartialMatchForTags),
            new(SearchTagMatchOption.ExactMatchForTags, MiscResources.SearchTagMatchOptionExactMatchForTags),
            new(SearchTagMatchOption.TitleAndCaption, MiscResources.SearchTagMatchOptionTitleAndCaption)
        };

        public static readonly IEnumerable<TargetFilterWrapper> AvailableTargetFilters = new TargetFilterWrapper[]
        {
            new(TargetFilter.ForAndroid, MiscResources.TargetFilterForAndroid),
            new(TargetFilter.ForIos, MiscResources.TargetFilterForIOS)
        };

        public static IllustrationSortOptionWrapper GetAppSettingDefaultSortOptionWrapper()
        {
            return AvailableIllustrationSortOptions.First(it => it.Value == App.AppSetting.DefaultSortOption);
        }

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

        public static string GetIllustrationWebUrl(string id)
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

        public static Action<IList<IllustrationViewModel>, IllustrationViewModel?>? Insert(IllustrationSortOption sortOption)
        {
            return sortOption switch
            {
                IllustrationSortOption.PublishDateAscending => (models, model) => models!.AddSorted(model, (m1, m2) => m1.Compare(m2, m => m.PublishDate)),
                IllustrationSortOption.PublishDateDescending => (models, model) => models!.AddSorted(model, (m1, m2) => -m1.Compare(m2, m => m.PublishDate)),
                IllustrationSortOption.PopularityDescending => (models, model) => models!.AddSorted(model, (m1, m2) => -m1.Compare(m2, m => m.Bookmark)),
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
    }
}