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
using Pixeval.Persisting;

namespace Pixeval.Core
{
    public class PixivHelper
    {
        public static async Task<Illustration> IllustrationInfo(string id)
        {
            IllustResponse.Response response;
            try
            {
                response = (await HttpClientFactory.PublicApiService.GetSingle(id)).ToResponse[0];
            }
            catch (Exception)
            {
                return null;
            }

            var illust = new Illustration
            {
                Bookmark = (int) (response.Stats.FavoritedCount.Private + response.Stats.FavoritedCount.Public),
                Id = response.Id.ToString(),
                IsLiked = response.FavoriteId != 0,
                IsManga = response.IsManga,
                IsUgoira = response.Type == "ugoira",
                Origin = response.ImageUrls.Large,
                Tags = response.Tags.ToArray(),
                Thumbnail = response.ImageUrls.Px480Mw ?? response.ImageUrls.Medium,
                Title = response.Title,
                Type = response.Type,
                UserName = response.User.Name,
                UserId = response.User.Id.ToString()
            }.Apply(i => i.TypeEnum = IllustTypeParser.ParseType(i));

            if (illust.IsManga)
                illust.MangaMetadata = response.Metadata.Pages.Select(p =>
                {
                    var page = (Illustration) illust.Clone();
                    page.IsManga = false;
                    page.Origin = p.ImageUrls.Large;
                    return page;
                }).ToArray();

            return illust;
        }

        internal static async void DoIterate<T>(IPixivIterator<T> pixivIterator, ICollection<T> container, bool useCounter = false)
        {
            var counter = 1;
            while (pixivIterator.HasNext())
            {
                if (useCounter && counter > Settings.Global.QueryPages * 10) break;
                await foreach (var illust in pixivIterator.MoveNextAsync())
                    if (typeof(T) == typeof(Illustration))
                    {
                        var i = illust as Illustration;
                        if (IllustNotMatchCondition(Settings.Global.ExceptTags, Settings.Global.ContainsTags, i))
                            continue;

                        if (container is Collection<Illustration> illustrationContainer)
                        {
                            illustrationContainer.AddSorted(i, Settings.Global.SortOnInserting ? IllustrationPopularityComparator.Instance : (IComparer<Illustration>) IllustrationPublishDateComparator.Instance);
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
            return !exceptTag.IsNullOrEmpty() && exceptTag.Any(x => !x.IsNullOrEmpty() && illustration.Tags.Any(i => i.EqualsIgnoreCase(x))) ||
                   !containsTag.IsNullOrEmpty() && containsTag.Any(x => !x.IsNullOrEmpty() && !illustration.Tags.Any(i => i.EqualsIgnoreCase(x))) ||
                   illustration.Bookmark < Settings.Global.MinBookmark;
        }
    }
}