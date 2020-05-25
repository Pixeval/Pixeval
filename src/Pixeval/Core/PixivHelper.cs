#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects.Exceptions.Logger;
using Pixeval.Objects.Generic;
using Pixeval.Objects.Primitive;
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
                response = (await HttpClientFactory.AppApiService().GetSingle(id)).IllustInfo;
            }
            catch (ApiException e)
            {
                ExceptionDumper.WriteException(e);
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

            if (illust.IsManga && response.MetaPages != null)
                illust.MangaMetadata = response.MetaPages.Select(p =>
                {
                    var page = (Illustration) illust.Clone();
                    page.Thumbnail = p.ImageUrls.Medium;
                    page.Origin = p.ImageUrls.Original;
                    page.Large = p.ImageUrls.Large;
                    return page;
                }).ToArray();

            return illust;
        }

        public static async void Enumerate<T>(IPixivAsyncEnumerable<T> pixivIterator, IList<T> container,
                                              int limit = -1)
        {
            EnumeratingSchedule.StartNewInstance(pixivIterator);
            var enumerator = EnumeratingSchedule.GetCurrentEnumerator<T>();

            await foreach (var illust in enumerator)
            {
                if (enumerator.IsCancellationRequested() || limit != -1 && pixivIterator.RequestedPages > limit) break;
                if (pixivIterator.VerifyRationality(illust, container))
                    pixivIterator.InsertionPolicy(illust, container);
            }
        }

        public static void RecordTimeline(ITimelineService service, BrowsingHistory browsingHistory)
        {
            if (service.VerifyRationality(browsingHistory)) service.Insert(browsingHistory);
        }

        public static void RecordTimelineInternal(BrowsingHistory browsingHistory)
        {
            RecordTimeline(BrowsingHistoryAccessor.GlobalLifeTimeScope, browsingHistory);
            if (CheckWindowsVersion()) RecordTimeline(WindowsUserActivityManager.GlobalLifeTimeScope, browsingHistory);

            static bool CheckWindowsVersion()
            { 
                return
                    Environment.OSVersion
                        .Version >= /* Windows 10 April 2018 Update(a.k.a. Redstone 4), which is the version that release Windows Timeline feature */
                    new Version(10, 0, 17134);
            }
        }

        public static bool VerifyIllustRational(ISet<string> excludeTag, ISet<string> includeTag, int minBookmark,
                                                Illustration illustration)
        {
            if (illustration == null) return false;
            bool excludeMatch = true, includeMatch = true;
            if (!excludeTag.IsNullOrEmpty())
                excludeMatch = excludeTag.All(x =>
                                                  x.IsNullOrEmpty() ||
                                                  illustration.Tags.All(i => !i.Name.EqualsIgnoreCase(x)));

            if (!includeTag.IsNullOrEmpty())
                includeMatch = includeTag.All(x =>
                                                  x.IsNullOrEmpty() ||
                                                  illustration.Tags.Any(i => i.Name.EqualsIgnoreCase(x)));

            var minBookmarkMatch = illustration.Bookmark > minBookmark;
            return excludeMatch && includeMatch && minBookmarkMatch;
        }
    }
}