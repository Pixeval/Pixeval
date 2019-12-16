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

namespace Pixeval.Core
{
    public class RankingIterator : IPixivIterator
    {
        private RankingResponse context;

        private int limit;

        public bool HasNext()
        {
            if (context == null) return true;

            if (limit++ >= 5) return false;

            return !context.NextUrl.IsNullOrEmpty();
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            var httpClient = HttpClientFactory.PixivApi(ProtocolBase.AppApiBaseUrl);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer");

            const string query = "/v1/illust/recommended";
            context = (await httpClient.GetStringAsync(context == null ? query : context.NextUrl)).FromJson<RankingResponse>();

            foreach (var contextIllust in context.Illusts.Where(illustration => illustration != null)) yield return await PixivHelper.IllustrationInfo(contextIllust.Id.ToString());
        }
    }
}