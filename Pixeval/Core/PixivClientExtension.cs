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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;

namespace Pixeval.Core
{
    public static class PixivClientExtension
    {
        public static async void PostFavoriteAsync(this PixivClient _, Illustration illustration)
        {
            illustration.IsLiked = true;
            await HttpClientFactory.AppApiService().AddBookmark(new AddBookmarkRequest {Id = illustration.Id});
        }

        public static async void RemoveFavoriteAsync(this PixivClient _, Illustration illustration)
        {
            illustration.IsLiked = false;
            await HttpClientFactory.AppApiService().DeleteBookmark(new DeleteBookmarkRequest {IllustId = illustration.Id});
        }

        public static async Task<IEnumerable<string>> GetArticleWorks(this PixivClient _, string spotlightId)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync($"https://www.pixivision.net/en/a/{spotlightId}");

            var doc = await new HtmlParser().ParseDocumentAsync(html);

            return doc.QuerySelectorAll(".am__body .am__work")
                .Select(element => element.Children[1].Children[0].GetAttribute("href"))
                .Select(url => Regex.Match(url, "https://www.pixiv.net/artworks/(?<Id>\\d+)").Groups["Id"].Value);
        }

        public static async Task FollowArtist(this PixivClient _, User user)
        {
            user.IsFollowed = true;
            await HttpClientFactory.AppApiService().FollowArtist(new FollowArtistRequest {Id = user.Id});
        }

        public static async Task UnFollowArtist(this PixivClient _, User user)
        {
            user.IsFollowed = false;
            await HttpClientFactory.AppApiService().UnFollowArtist(new UnFollowArtistRequest {UserId = user.Id});
        }
    }
}