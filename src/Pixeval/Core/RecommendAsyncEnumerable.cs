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
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;

namespace Pixeval.Core
{
    public abstract class AbstractRecommendAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        public override int RequestedPages { get; protected set; }

        public abstract override void InsertionPolicy(Illustration item, IList<Illustration> collection);

        public override bool VerifyRationality(Illustration item, IList<Illustration> collection)
        {
            return item != null && collection.All(t => t.Id != item.Id) && PixivHelper.VerifyIllustRational(Settings.Global.ExcludeTag, Settings.Global.IncludeTag, Settings.Global.MinBookmark, item);
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new RecommendAsyncEnumerator(this);
        }

        private class RecommendAsyncEnumerator : AbstractPixivAsyncEnumerator<Illustration>
        {
            private RecommendResponse _entity;

            private IEnumerator<Illustration> _illustrationEnumerator;

            public RecommendAsyncEnumerator(IPixivAsyncEnumerable<Illustration> enumerable) : base(enumerable)
            {
            }

            public override Illustration Current => _illustrationEnumerator.Current;

            protected override void UpdateEnumerator()
            {
                _illustrationEnumerator = _entity.Illusts.NonNull().Select(_ => _.Parse()).GetEnumerator();
            }

            public override async ValueTask<bool> MoveNextAsync()
            {
                if (_entity == null)
                {
                    if (await TryGetResponse("/v1/illust/recommended") is (true, var model))
                    {
                        _entity = model;
                        UpdateEnumerator();
                    }
                    else
                    {
                        throw new QueryNotRespondingException();
                    }

                    Enumerable.ReportRequestedPages();
                }

                if (_illustrationEnumerator.MoveNext()) return true;

                if (_entity.NextUrl.IsNullOrEmpty()) return false;

                if (await TryGetResponse(_entity.NextUrl) is (true, var res))
                {
                    _entity = res;
                    UpdateEnumerator();
                    Enumerable.ReportRequestedPages();
                    return true;
                }

                return false;
            }

            private static async Task<HttpResponse<RecommendResponse>> TryGetResponse(string url)
            {
                var res = (await HttpClientFactory.AppApiHttpClient().Apply(h => h.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", AkaI18N.GetCultureAcceptLanguage())).GetStringAsync(url)).FromJson<RecommendResponse>();
                if (res is { } response && !response.Illusts.IsNullOrEmpty()) return HttpResponse<RecommendResponse>.Wrap(true, response);

                return HttpResponse<RecommendResponse>.Wrap(false);
            }
        }
    }

    public class PopularityRecommendAsyncEnumerable : AbstractRecommendAsyncEnumerable
    {
        public override void InsertionPolicy(Illustration item, IList<Illustration> collection)
        {
            if (item != null) collection.AddSorted(item, IllustrationPopularityComparator.Instance);
        }
    }

    public class PublishDateRecommendAsyncEnumerable : AbstractRecommendAsyncEnumerable
    {
        public override void InsertionPolicy(Illustration item, IList<Illustration> collection)
        {
            if (item != null) collection.AddSorted(item, IllustrationPublishDateComparator.Instance);
        }
    }
}