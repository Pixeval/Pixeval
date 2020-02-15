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
using Pixeval.Objects.Exceptions;

namespace Pixeval.Core
{
    public class UserPreviewIterator : IPixivIterator<User>
    {
        private readonly string keyword;

        private UserNavResponse context;

        private int counter;

        public UserPreviewIterator(string keyword)
        {
            this.keyword = keyword;
        }

        public SortOption SortOption { get; } = SortOption.None;

        public bool HasNext()
        {
            if (context == null) return true;

            return counter <= 10 && !context.NextUrl.IsNullOrEmpty();
        }

        public async IAsyncEnumerable<User> MoveNextAsync()
        {
            var url = $"https://app-api.pixiv.net/v1/search/user?filter=for_android&word={keyword}";
            context = (await HttpClientFactory.AppApiHttpClient.GetStringAsync(context == null ? url : context.NextUrl)).FromJson<UserNavResponse>();

            if (context.UserPreviews.IsNullOrEmpty() && counter++ == 0) throw new QueryNotRespondingException();

            foreach (var responseUserPreview in context.UserPreviews.Where(u => u != null))
            {
                var usr = new User
                {
                    Avatar = responseUserPreview.User.ProfileImageUrls.Medium,
                    Thumbnails = responseUserPreview.Illusts.Select(i => i.ImageUrl.SquareMedium).ToArray(),
                    Id = responseUserPreview.User.Id.ToString(),
                    Name = responseUserPreview.User.Name
                };
                yield return usr;
            }
        }
    }
}