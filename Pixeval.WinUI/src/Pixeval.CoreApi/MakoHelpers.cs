using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Preference;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi
{
    [PublicAPI]
    public static class MakoHelpers
    {
        public static bool Satisfies(this Illustration? item, IEnumerable<Illustration> collection, MakoClient makoClient)
        {
            return item is not null && collection.All(i => i.Id != item.Id) && item.Satisfies(makoClient.Configuration.ExcludeTags, makoClient.Configuration.IncludeTags, makoClient.Configuration.MinBookmark);
        }
        
        public static bool Satisfies(this Illustration illustration, IEnumerable<string>? excludeTags, IEnumerable<string>? includeTags, int minBookmarks)
        {
            if (illustration.Bookmarks <= minBookmarks)
            {
                return false;
            }

            if (illustration.Tags is { } tags)
            {
                var tagArr = tags as Tag[] ?? tags.ToArray();
                if (excludeTags is not null && excludeTags.Intersect(tagArr.Select(t => t.Name).WhereNotNull(), Objects.CaseIgnoredComparer).Any())
                {
                    return false;
                }

                if (includeTags is not null && !includeTags.SequenceEquals(tagArr.Select(t => t.Name).WhereNotNull(), SequenceComparison.Unordered, Objects.CaseIgnoredComparer))
                {
                    return false;
                }
            }

            return illustration.Bookmarks >= minBookmarks;
        }

        internal static Illustration ToIllustration(this IllustrationEssential.Illust illust, MakoClient cacheProvider)
        {
            var illustration = Illustration.GetOrInstantiateAndConfigureIllustrationFromCache(illust.Id.ToString(), cacheProvider, i =>
            {
                i.Bookmarks = illust.TotalBookmarks;
                i.Id = illust.Id.ToString();
                i.IsBookmarked = illust.IsBookmarked;
                i.IsManga = illust.PageCount != 1;
                i.IsUgoira = illust.Type == "ugoira";
                i.OriginalUrl = illust.ImageUrls?.Original ?? illust.MetaSinglePage?.OriginalImageUrl;
                i.LargeUrl = illust.ImageUrls?.Large;
                i.Tags = illust.Tags?.Select(t => new Tag {Name = t.Name, TranslatedName = t.TranslatedName});
                i.ThumbnailUrl = illust.ImageUrls?.Medium.IsNullOrEmpty() ?? true ? illust.ImageUrls?.SquareMedium : illust.ImageUrls.Medium;
                i.Title = illust.Title;
                i.ArtistId = illust.User?.Id.ToString();
                i.ArtistName = illust.User?.Name;
                i.Resolution = new Resolution(illust.Width, illust.Height);
                i.TotalViews = illust.TotalView;
                i.PublishDate = illust.CreateDate;
            });
            if (illustration.IsManga)
            {
                illustration.MangaMetadata = illust.MetaPages?.Select(mp => illustration with
                {
                    ThumbnailUrl = mp.ImageUrls?.Medium,
                    OriginalUrl = mp.ImageUrls?.Original,
                    LargeUrl = mp.ImageUrls?.Large
                }).ToArray();

                if (!illustration.MangaMetadata?.Any() ?? true)
                {
                    throw new MangaPagesNotFoundException(illustration);
                }

                foreach (var i in illustration.MangaMetadata!)
                {
                    i.MangaMetadata = illustration.MangaMetadata;
                }
            }

            return illustration;
        }

        public static Session ToSession(this TokenResponse tokenResponse)
        {
            return new()
            {
                AccessToken = tokenResponse.AccessToken,
                Account = tokenResponse.User?.Account,
                AvatarUrl = tokenResponse.User?.ProfileImageUrls?.Px170X170,
                ExpireIn = DateTime.Now + TimeSpan.FromSeconds(tokenResponse.ExpiresIn) - TimeSpan.FromMinutes(5), // 减去5分钟是考虑到网络延迟会导致精确时间不可能恰好是一小时(TokenResponse的ExpireIn是60分钟)
                Id = tokenResponse.User?.Id,
                IsPremium = tokenResponse.User?.IsPremium ?? false,
                RefreshToken = tokenResponse.RefreshToken,
                Name = tokenResponse.User?.Name
            };
        }
    }
}