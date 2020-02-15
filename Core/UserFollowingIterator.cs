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
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;

namespace Pixeval.Core
{
    public class UserFollowingIterator : IPixivIterator<User>
    {
        private readonly string uid;

        private FollowingResponse context;

        public UserFollowingIterator(string userId)
        {
            uid = userId;
        }

        public SortOption SortOption { get; } = SortOption.None;

        public bool HasNext()
        {
            if (context == null) return true;

            return !context.NextUrl.IsNullOrEmpty();
        }

        public async IAsyncEnumerable<User> MoveNextAsync()
        {
            var url = $"https://app-api.pixiv.net/v1/user/following?user_id={uid}&restrict=public";
            context = (await HttpClientFactory.AppApiHttpClient.GetStringAsync(context == null ? url : context.NextUrl)).FromJson<FollowingResponse>();

            foreach (var preview in context.UserPreviews.Where(u => u != null))
            {
                var usr = new User
                {
                    Thumbnails = preview.Illusts.Select(i => i.ImageUrls.SquareMedium).ToArray(),
                    Id = preview.User.Id.ToString(),
                    Name = preview.User.Name,
                    Avatar = preview.User.ProfileImageUrls.Medium
                };
                yield return usr;
            }
        }
    }
}