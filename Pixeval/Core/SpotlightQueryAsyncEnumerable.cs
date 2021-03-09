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
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects.Exceptions;
using Pixeval.Objects.Generic;
using Pixeval.Objects.Primitive;

namespace Pixeval.Core
{
    public class SpotlightQueryAsyncEnumerable : AbstractPixivAsyncEnumerable<SpotlightArticle>
    {
        private readonly int start;

        public SpotlightQueryAsyncEnumerable(int start)
        {
            this.start = start < 1 ? 1 : start;
        }

        public override int RequestedPages { get; protected set; }

        public override IAsyncEnumerator<SpotlightArticle> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new SpotlightArticleAsyncEnumerator(this, start);
        }

        private class SpotlightArticleAsyncEnumerator : AbstractPixivAsyncEnumerator<SpotlightArticle>
        {
            private int current;

            private SpotlightResponse entity;

            private IEnumerator<SpotlightArticle> spotlightArticleEnumerator;

            public SpotlightArticleAsyncEnumerator(IPixivAsyncEnumerable<SpotlightArticle> enumerable, int current) : base(enumerable)
            {
                this.current = current;
            }

            public override SpotlightArticle Current => spotlightArticleEnumerator.Current;

            protected override void UpdateEnumerator()
            {
                spotlightArticleEnumerator = entity.SpotlightArticles.NonNull().GetEnumerator();
            }

            public override async ValueTask<bool> MoveNextAsync()
            {
                if (entity == null)
                {
                    if (await TryGetResponse() is (true, var model))
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

                if (spotlightArticleEnumerator.MoveNext())
                {
                    return true;
                }

                if (entity.NextUrl.IsNullOrEmpty())
                {
                    return false;
                }

                if (await TryGetResponse() is (true, var res))
                {
                    entity = res;
                    UpdateEnumerator();
                    Enumerable.ReportRequestedPages();
                    return true;
                }

                return false;
            }

            private async Task<HttpResponse<SpotlightResponse>> TryGetResponse()
            {
                var res = await HttpClientFactory.AppApiService.GetSpotlights(current++ * 10);

                if (res is { } response && !response.SpotlightArticles.IsNullOrEmpty())
                {
                    return HttpResponse<SpotlightResponse>.Wrap(true, response);
                }

                return HttpResponse<SpotlightResponse>.Wrap(false);
            }
        }
    }
}