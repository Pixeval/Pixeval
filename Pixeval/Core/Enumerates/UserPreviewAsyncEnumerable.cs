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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects.Exceptions;
using Pixeval.Objects.Generic;
using Pixeval.Objects.Primitive;

namespace Pixeval.Core.Enumerates
{
    public class UserPreviewAsyncEnumerable : AbstractPixivAsyncEnumerable<User>
    {
        private readonly string keyword;

        public UserPreviewAsyncEnumerable(string keyword)
        {
            this.keyword = keyword;
        }

        public override int RequestedPages { get; protected set; }

        public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new UserPreviewAsyncEnumerator(this, keyword);
        }

        private class UserPreviewAsyncEnumerator : AbstractPixivAsyncEnumerator<User>
        {
            private readonly string keyword;
            private UserNavResponse entity;

            private IEnumerator<User> userPreviewEnumerator;

            public UserPreviewAsyncEnumerator(IPixivAsyncEnumerable<User> enumerable, string keyword) : base(enumerable)
            {
                this.keyword = keyword;
            }

            public override User Current => userPreviewEnumerator.Current;

            protected override void UpdateEnumerator()
            {
                userPreviewEnumerator = entity.UserPreviews.NonNull().Select(u => new User
                {
                    Avatar = u.User.ProfileImageUrls.Medium,
                    Thumbnails = u.Illusts.NonNull().Select(_ => _.ImageUrl.SquareMedium).ToArray(),
                    Id = u.User.Id.ToString(),
                    Name = u.User.Name
                }).GetEnumerator();
            }

            public override async ValueTask<bool> MoveNextAsync()
            {
                if (entity == null)
                {
                    if (await TryGetResponse($"https://app-api.pixiv.net/v1/search/user?filter=for_android&word={keyword}") is (true, var model))
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

                if (userPreviewEnumerator.MoveNext())
                {
                    return true;
                }

                if (entity.NextUrl.IsNullOrEmpty())
                {
                    return false;
                }

                if (await TryGetResponse(entity.NextUrl) is (true, var res))
                {
                    entity = res;
                    UpdateEnumerator();
                    Enumerable.ReportRequestedPages();
                    return true;
                }

                return false;
            }

            private static async Task<HttpResponse<UserNavResponse>> TryGetResponse(string url)
            {
                var res = (await HttpClientFactory.AppApiHttpClient().GetStringAsync(url)).FromJson<UserNavResponse>();
                if (res is { } response && !response.UserPreviews.IsNullOrEmpty())
                {
                    return HttpResponse<UserNavResponse>.Wrap(true, response);
                }

                return HttpResponse<UserNavResponse>.Failure;
            }
        }
    }
}