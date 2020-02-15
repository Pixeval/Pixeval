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
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;

namespace Pixeval.Core
{
    public class UserUpdateIterator : IPixivIterator<Illustration>
    {
        private UserUpdateResponse context;

        public SortOption SortOption { get; } = SortOption.None;

        public bool HasNext()
        {
            if (context == null) return true;

            return !context.NextUrl.IsNullOrEmpty();
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            const string url = "https://app-api.pixiv.net/v2/illust/follow?restrict=public";
            var response = (await HttpClientFactory.AppApiHttpClient.GetStringAsync(context == null ? url : context.NextUrl)).FromJson<UserUpdateResponse>();

            if (response.Illusts.IsNullOrEmpty() && context == null) throw new QueryNotRespondingException();

            context = response;
            foreach (var illust in context.Illusts) yield return illust.Parse();
        }
    }
}