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

using System;
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
using Pixeval.Persisting;

namespace Pixeval.Core
{
    public class RankingAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        private readonly DateTime dateTime;
        private readonly RankOption rankOption;

        public RankingAsyncEnumerable(RankOption rankOption, DateTime dateTime)
        {
            this.rankOption = rankOption;
            this.dateTime = dateTime;
        }

        public override int RequestedPages { get; protected set; }

        public override bool VerifyRationality(Illustration item, IList<Illustration> collection)
        {
            return item != null && collection.All(t => t.Id != item.Id) && PixivHelper.VerifyIllust(Settings.Global.ExcludeTag, Settings.Global.IncludeTag, Settings.Global.MinBookmark, item);
        }

        public override void InsertionPolicy(Illustration item, IList<Illustration> collection)
        {
            collection.Add(item);
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new RankingAsyncEnumerator(this, rankOption, dateTime);
        }

        private class RankingAsyncEnumerator : AbstractPixivAsyncEnumerator<Illustration>
        {
            private readonly string dateTimeParameter;
            private readonly string rankOptionParameter;
            private RankingResponse entity;

            private IEnumerator<Illustration> illustrationEnumerator;

            public RankingAsyncEnumerator(IPixivAsyncEnumerable<Illustration> enumerable, RankOption rankOption, DateTime dateTime) : base(enumerable)
            {
                rankOptionParameter = rankOption.GetEnumAttribute<EnumAlias>().AliasAs;
                dateTimeParameter = dateTime.ToString("yyyy-MM-dd");
            }

            public override Illustration Current => illustrationEnumerator.Current;

            protected override void UpdateEnumerator()
            {
                illustrationEnumerator = entity.Illusts.NonNull().Select(_ => _.Parse()).GetEnumerator();
            }

            public override async ValueTask<bool> MoveNextAsync()
            {
                if (entity == null)
                {
                    if (await TryGetResponse($"/v1/illust/ranking?filter=for_android&mode={rankOptionParameter}&date={dateTimeParameter}") is (true, var result))
                    {
                        entity = result;
                        UpdateEnumerator();
                    }
                    else
                    {
                        throw new QueryNotRespondingException();
                    }

                    Enumerable.ReportRequestedPages();
                }

                if (illustrationEnumerator.MoveNext())
                {
                    return true;
                }

                if (entity.NextUrl.IsNullOrEmpty())
                {
                    return false;
                }

                if (await TryGetResponse(entity.NextUrl) is (true, var model))
                {
                    entity = model;
                    UpdateEnumerator();
                    Enumerable.ReportRequestedPages();
                    return true;
                }

                return false;
            }

            private static async Task<HttpResponse<RankingResponse>> TryGetResponse(string url)
            {
                var result = (await HttpClientFactory.AppApiHttpClient().GetStringAsync(url)).FromJson<RankingResponse>();

                if (result is { } response && !response.Illusts.IsNullOrEmpty())
                {
                    return HttpResponse<RankingResponse>.Wrap(true, response);
                }
                return HttpResponse<RankingResponse>.Wrap(false);
            }
        }
    }
}