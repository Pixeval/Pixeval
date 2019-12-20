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
using Pixeval.Data.Web.Request;
using Pixeval.Objects.Exceptions;

namespace Pixeval.Core
{
    public class UploadIterator : IPixivIterator<Illustration>
    {
        private readonly string uid;

        private int currentIndex = 1;

        public UploadIterator(string uid)
        {
            this.uid = uid;
        }

        public bool HasNext()
        {
            return true;
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            var works = await HttpClientFactory.PublicApiService.GetUploads(uid, new UploadsRequest {Page = currentIndex++});
            if (currentIndex == 2 && !works.ToResponse.Any()) throw new QueryNotRespondingException();

            foreach (var response in works.ToResponse.Where(illustration => illustration != null))
            {
                var illust = await PixivHelper.IllustrationInfo(response.Id.ToString());
                if (illust != null) yield return illust;
            }
        }
    }
}