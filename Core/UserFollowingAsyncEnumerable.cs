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
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;

namespace Pixeval.Core
{
    public class UserFollowingAsyncEnumerable : AbstractPixivAsyncEnumerable<User>
    {
        private readonly string uid;

        public UserFollowingAsyncEnumerable(string userId)
        {
            uid = userId;
        }

        public override SortOption SortOption { get; } = SortOption.None;

        public override int RequestedPages { get; protected set; }

        public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new UserFollowingAsyncEnumerator(this, uid);
        }

        private class UserFollowingAsyncEnumerator : AbstractPixivAsyncEnumerator<User>
        {
            private readonly string userId;

            private FollowingResponse entity;

            private IEnumerator<User> followerEnumerator;

            public UserFollowingAsyncEnumerator(IPixivAsyncEnumerable<User> enumerable, string userId) : base(enumerable)
            {
                this.userId = userId;
            }

            public override User Current => followerEnumerator.Current;

            protected override void UpdateEnumerator()
            {
                followerEnumerator = entity.UserPreviews.NonNull().Select(u => new User
                {
                    Thumbnails = u.Illusts.NonNull().Select(_ => _.ImageUrls.SquareMedium).ToArray(),
                    Id = u.User.Id.ToString(),
                    Name = u.User.Name,
                    Avatar = u.User.ProfileImageUrls.Medium
                }).GetEnumerator();
            }

            public override async ValueTask<bool> MoveNextAsync()
            {
                if (entity == null)
                {
                    if (await TryGetResponse($"https://app-api.pixiv.net/v1/user/following?user_id={userId}&restrict=public") is (true, var model))
                    {
                        entity = model;
                        UpdateEnumerator();
                    }
                    else
                    {
                        throw new QueryNotRespondingException();
                    }

                    Enumerable.ReportRequestedPages();
                }

                if (followerEnumerator.MoveNext()) return true;

                if (entity.NextUrl.IsNullOrEmpty()) return false;

                if (await TryGetResponse(entity.NextUrl) is (true, var res))
                {
                    entity = res;
                    UpdateEnumerator();
                    Enumerable.ReportRequestedPages();
                    return true;
                }

                return false;
            }

            private static async Task<HttpResponse<FollowingResponse>> TryGetResponse(string url)
            {
                var res = (await HttpClientFactory.AppApiHttpClient.GetStringAsync(url)).FromJson<FollowingResponse>();
                if (res is { } response && !response.UserPreviews.IsNullOrEmpty()) return HttpResponse<FollowingResponse>.Wrap(true, response);

                return HttpResponse<FollowingResponse>.Wrap(false);
            }
        }
    }
}