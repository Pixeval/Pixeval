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
    public class GalleryIterator : IPixivIterator<Illustration>
    {
        private readonly string uid;

        private GalleryResponse context;

        public GalleryIterator(string uid)
        {
            this.uid = uid;
        }

        public SortOption SortOption { get; } = SortOption.None;

        public bool HasNext()
        {
            if (context == null) return true;
            return !context.NextUrl.IsNullOrEmpty();
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            var query = $"/v1/user/bookmarks/illust?user_id={uid}&restrict=public&filter=for_ios";

            var model = (await HttpClientFactory.AppApiHttpClient.GetStringAsync(context == null ? query : context.NextUrl)).FromJson<GalleryResponse>();

            if (context == null && model.Illusts.IsNullOrEmpty()) throw new QueryNotRespondingException();

            context = model;

            foreach (var contextIllust in context.Illusts.Where(contextIllust => contextIllust != null)) yield return contextIllust.Parse();
        }
    }
}