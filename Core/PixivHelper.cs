// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions.Logger;
using Pixeval.Persisting;
using Refit;

namespace Pixeval.Core
{
    public class PixivHelper
    {
        public static async Task<Illustration> IllustrationInfo(string id)
        {
            SingleWorkResponse.Illust response;
            try
            {
                response = (await HttpClientFactory.AppApiService.GetSingle(id)).IllustInfo;
            }
            catch (ApiException e)
            {
                ExceptionLogger.WriteException(e);
                return null;
            }
            catch (Exception)
            {
                return null;
            }

            var illust = new Illustration
            {
                Bookmark = (int) response.TotalBookmarks,
                Id = response.Id.ToString(),
                IsLiked = response.IsBookmarked,
                IsManga = response.PageCount != 1,
                IsUgoira = response.Type == "ugoira",
                Origin = response.ImageUrls.Original,
                Large = response.ImageUrls.Large,
                Tags = response.Tags.Select(t => new Tag {Name = t.Name, TranslatedName = t.TranslatedName}),
                Thumbnail = response.ImageUrls.Medium,
                Title = response.Title,
                UserName = response.User.Name,
                UserId = response.User.Id.ToString(),
                ViewCount = (int) response.TotalView,
                Comments = (int) response.TotalComments,
                Resolution = $"{response.Width}x{response.Height}",
                PublishDate = response.CreateDate
            };

            if (illust.IsManga)
                illust.MangaMetadata = response.MetaPages.Select(p =>
                {
                    var page = (Illustration) illust.Clone();
                    page.Origin = p.ImageUrls.Original;
                    page.Large = p.ImageUrls.Large;
                    return page;
                }).ToArray();

            return illust;
        }

        internal static async void DoIterate<T>(IPixivIterator<T> pixivIterator, ICollection<T> container, bool useCounter = false)
        {
            var counter = 1;
            var hashTable = new Dictionary<string, Illustration>();
            while (pixivIterator.HasNext())
            {
                if (useCounter && counter > Settings.Global.QueryPages * 10) break;
                await foreach (var illust in pixivIterator.MoveNextAsync())
                    if (illust is Illustration i)
                    {
                        if (IllustNotMatchCondition(Settings.Global.ExceptTags, Settings.Global.ContainsTags, i))
                            continue;

                        if (container is Collection<Illustration> illustrationContainer)
                        {
                            if (hashTable.ContainsKey(i.Id)) continue;

                            hashTable[i.Id] = i;
                            IComparer<Illustration> comparer = pixivIterator.SortOption switch
                            {
                                SortOption.None        => null,
                                SortOption.PublishDate => IllustrationPublishDateComparator.Instance,
                                SortOption.Popularity  => IllustrationPopularityComparator.Instance,
                                _                      => null
                            };
                            illustrationContainer.AddSorted(i, comparer);
                        }
                    }
                    else
                    {
                        container.Add(illust);
                    }

                counter++;
            }
        }

        internal static bool IllustNotMatchCondition(ISet<string> exceptTag, ISet<string> containsTag, Illustration illustration)
        {
            if (illustration == null) return false;
            return !exceptTag.IsNullOrEmpty() && exceptTag.Any(x => !x.IsNullOrEmpty() && illustration.Tags.Any(i => i.Name.EqualsIgnoreCase(x))) ||
                   !containsTag.IsNullOrEmpty() && containsTag.Any(x => !x.IsNullOrEmpty() && !illustration.Tags.Any(i => i.Name.EqualsIgnoreCase(x))) ||
                   illustration.Bookmark < Settings.Global.MinBookmark;
        }
    }
}