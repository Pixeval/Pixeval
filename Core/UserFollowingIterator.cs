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
using System.Collections.Generic;
using System.Linq;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Pixeval.Persisting;

namespace Pixeval.Core
{
    public class UserFollowingIterator
    {
        private readonly string userId;

        private string url;

        public UserFollowingIterator(string userId)
        {
            this.userId = userId;
            url = $"https://app-api.pixiv.net/v1/user/following?user_id={userId}&restrict=public";
        }

        public async IAsyncEnumerable<User> GetUserPreviews()
        {
            var client = HttpClientFactory.PixivApi(ProtocolBase.AppApiBaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Identity.Global.AccessToken}");

            while (!url.IsNullOrEmpty())
            {
                var response = (await client.GetStringAsync(url)).FromJson<FollowingResponse>();
                foreach (var preview in response.UserPreviews)
                    yield return new User
                    {
                        Thumbnails = preview.Illusts.Select(i => i.ImageUrls.SquareMedium).ToArray(),
                        Id = preview.User.Id.ToString(),
                        Name = preview.User.Name,
                        Avatar = preview.User.ProfileImageUrls.Medium
                    };

                url = response.NextUrl;
            }
        }
    }
}