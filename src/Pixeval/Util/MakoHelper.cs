using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI.UI;
using Mako.Engine;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.ViewModel;

namespace Pixeval.Util
{
    public enum XRestrictLevel
    {
        [Metadata("")]
        Ordinary = 0,

        [Metadata("R-18")]
        R18 = 1, 

        [Metadata("R-18G")]
        R18G = 2
    }

    public enum ThumbnailUrlOption
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
        /// so an alternative is chosen
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