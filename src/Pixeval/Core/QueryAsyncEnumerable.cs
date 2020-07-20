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
    public abstract class AbstractQueryAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        protected readonly bool IsPremium;
        private readonly SearchTagMatchOption _matchOption;
        private readonly int _start;
        private readonly string _tag;

        protected AbstractQueryAsyncEnumerable(string tag, SearchTagMatchOption matchOption, bool isPremium,
                                               int start = 1)
        {
            this._start = start < 1 ? 1 : start;
            this._tag = tag;
            this._matchOption = matchOption;
            IsPremium = isPremium;
        }

        public override int RequestedPages { get; protected set; }

        public abstract override void InsertionPolicy(Illustration item, IList<Illustration> collection);

        public override bool VerifyRationality(Illustration item, IList<Illustration> collection)
        {
            return item != null &&
                collection.All(t => t.Id != item.Id) &&
                PixivHelper.VerifyIllustRational(Settings.Global.ExcludeTag, Settings.Global.IncludeTag,
                                                 Settings.Global.MinBookmark, item);
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new QueryAsyncEnumerator(this, _tag, _matchOption, _start, IsPremium);
        }

        private class QueryAsyncEnumerator : AbstractPixivAsyncEnumerator<Illustration>
        {
            private readonly int _current;
            private readonly bool _isPremium;
            private readonly string _keyword;
            private readonly SearchTagMatchOption _matchOption;

            private QueryWorksResponse _entity;

            private IEnumerator<Illustration> _illustrationsEnumerator;

            public QueryAsyncEnumerator(IPixivAsyncEnumerable<Illustration> enumerable, string keyword,
                                        SearchTagMatchOption matchOption, int current, bool isPremium) : base(enumerable)
            {
                this._keyword = keyword;
                this._matchOption = matchOption;
                this._current = current;
                this._isPremium = isPremium;
            }

            public override Illustration Current => _illustrationsEnumerator.Current;

            protected override void UpdateEnumerator()
            {
                _illustrationsEnumerator = _entity.Illusts.NonNull().Select(_ => _.Parse()).GetEnumerator();
            }

            public override async ValueTask<bool> MoveNextAsync()
            {
                if (_entity == null)
                {
                    if (await TryGetResponse(
                            $"/v1/search/illust?search_target={_matchOption.GetEnumAttribute<EnumAlias>().AliasAs}&sort={(_isPremium ? "date_desc" : "popular_desc")}&word={_keyword}&filter=for_android&offset={(_current - 1) * 30}")
                        is (true, var model))
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

                if (_illustrationsEnumerator.MoveNext()) return true;

                if (int.Parse(_entity.NextUrl[(_entity.NextUrl.LastIndexOf('=') + 1)..]) >= 5000) return false;

                if (await TryGetResponse(_entity.NextUrl) is (true, var res))
                {
                    _entity = res;
                    UpdateEnumerator();
                    Enumerable.ReportRequestedPages();
                    return true;
                }

                return false;
            }

            private static async Task<HttpResponse<QueryWorksResponse>> TryGetResponse(string url)
            {
                var res = (await HttpClientFactory.AppApiHttpClient()
                    .Apply(h => h.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", AkaI18N.GetCultureAcceptLanguage()))
                    .GetStringAsync(url)).FromJson<QueryWorksResponse>();
                if (res is { } response && !response.Illusts.IsNullOrEmpty()) return HttpResponse<QueryWorksResponse>.Wrap(true, response);

                return HttpResponse<QueryWorksResponse>.Wrap(false);
            }
        }
    }

    public class PopularityQueryAsyncEnumerable : AbstractQueryAsyncEnumerable
    {
        public PopularityQueryAsyncEnumerable(string tag, SearchTagMatchOption matchOption, bool isPremium,
                                              int start = 1) : base(tag, matchOption, isPremium, start)
        {
        }

        public override void InsertionPolicy(Illustration item, IList<Illustration> collection)
        {
            if (item != null)
            {
                if (IsPremium)
                    collection.Add(item);
                else
                    collection.AddSorted(item, IllustrationPopularityComparator.Instance);
            }
        }
    }

    public class PublishDateQueryAsyncEnumerable : AbstractQueryAsyncEnumerable
    {
        public PublishDateQueryAsyncEnumerable(string tag, SearchTagMatchOption matchOption, bool isPremium,
                                               int start = 1) : base(tag, matchOption, isPremium, start)
        {
        }

        public override void InsertionPolicy(Illustration item, IList<Illustration> collection)
        {
            if (item != null)
            {
                if (IsPremium)
                    collection.Add(item);
                else
                    collection.AddSorted(item, IllustrationPopularityComparator.Instance);
            }
        }
    }
}
