using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi;
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Options;
using Pixeval.Util.Generic;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.Util
{
    public static class MakoHelper
    {
        public static IllustrationSortOptionWrapper GetAppSettingDefaultSortOptionWrapper()
        {
            return IllustrationSortOptionWrapper.AvailableOptions().Of(App.AppSetting.DefaultSortOption);
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

        public static Uri GetIllustrationWebUri(string id)
        {
            return new($"https://www.pixiv.net/artworks/{id}");
        }

        public static string? GetOriginalUrl(this Illustration illustration)
        {
            return illustration.ImageUrls?.Original ?? illustration.MetaSinglePage?.OriginalImageUrl;
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

        public static async Task<SoftwareBitmapSource> DownloadSoftwareBitmapSourceAsync(this MakoClient client, string url)
        {
            return await (await client.GetMakoHttpClient(MakoApiKind.ImageApi)
                    .DownloadAsIRandomAccessStreamAsync(url))
                .GetOrThrow()
                .GetSoftwareBitmapSourceAsync(true);
        }
    }
}