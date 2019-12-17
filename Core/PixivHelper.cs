// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Data.Web.Response;

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
                Type = Illustration.IllustType.Parse(response),
                UserName = response.User.Name,
                UserId = response.User.Id.ToString()
            };

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

        public static async Task<int> GetUploadPagesCount(string uid)
        {
            return (int) (await HttpClientFactory.PublicApiService.GetUploads(uid, new UploadsRequest {Page = 1, PerPage = 1}))
                .UploadPagination
                .Pages;
        }

        public static async Task<int> GetQueryPagesCount(string tag)
        {
            var total = (double) (await HttpClientFactory.PublicApiService.QueryWorks(new QueryWorksRequest {Tag = tag, Offset = 1, PerPage = 1}))
                        .QueryPagination
                        .Pages / 300;

            return (int) Math.Ceiling(total);
        }
    }
}