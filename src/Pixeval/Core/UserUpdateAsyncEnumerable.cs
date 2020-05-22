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
    public class UserUpdateAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        public override int RequestedPages { get; protected set; }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new UserUpdateAsyncEnumerator(this);
        }

        public override bool VerifyRationality(Illustration item, IList<Illustration> collection)
        {
            return item != null &&
                collection.All(t => t.Id != item.Id) &&
                PixivHelper.VerifyIllustRational(Settings.Global.ExcludeTag, Settings.Global.IncludeTag,
                                                 Settings.Global.MinBookmark, item);
        }

        private class UserUpdateAsyncEnumerator : AbstractPixivAsyncEnumerator<Illustration>
        {
            private UserUpdateResponse entity;

            private IEnumerator<Illustration> illustrationEnumerator;

            public UserUpdateAsyncEnumerator(IPixivAsyncEnumerable<Illustration> enumerable) : base(enumerable)
            {
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
                    if (await TryGetResponse("https://app-api.pixiv.net/v2/illust/follow?restrict=public") is (true, var
                        model))
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

                if (illustrationEnumerator.MoveNext()) return true;

                if (entity.NextUrl.IsNullOrEmpty()) return false;

                if (await TryGetResponse(entity.NextUrl) is (true, var res))
                {
                    entity = res;
                    UpdateEnumerator();
                    Enumerable.ReportRequestedPages();
                    return true;
                }

                return false;
            }

            private static async Task<HttpResponse<UserUpdateResponse>> TryGetResponse(string url)
            {
                var res = (await HttpClientFactory.AppApiHttpClient()
                    .Apply(h => h.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", AkaI18N.GetCultureAcceptLanguage()))
                    .GetStringAsync(url)).FromJson<UserUpdateResponse>();
                if (res is { } response && !response.Illusts.IsNullOrEmpty())
                    return HttpResponse<UserUpdateResponse>.Wrap(true, response);

                return HttpResponse<UserUpdateResponse>.Wrap(false);
            }
        }
    }
}