﻿#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

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
    public abstract class AbstractGalleryAsyncEnumerable : AbstractPixivAsyncEnumerable<Illustration>
    {
        protected abstract string Uid { get; }

        protected abstract RestrictPolicy RestrictPolicy { get; }

        public override int RequestedPages { get; protected set; }

        public override bool Verify(Illustration item, IList<Illustration> collection)
        {
            return item != null && collection.All(t => t.Id != item.Id) && PixivHelper.VerifyIllust(Settings.Global.ExcludeTag, Settings.Global.IncludeTag, Settings.Global.MinBookmark, item);
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new GalleryAsyncEnumerator(Uid, this, RestrictPolicy);
        }

        public static AbstractGalleryAsyncEnumerable Of(string uid, RestrictPolicy restrictPolicy)
        {
            return restrictPolicy switch
            {
                RestrictPolicy.Public  => new PublicGalleryAsyncEnumerable(uid),
                RestrictPolicy.Private => new PrivateGalleryAsyncEnumerable(uid),
                _                      => throw new ArgumentOutOfRangeException(nameof(restrictPolicy), restrictPolicy, null)
            };
        }

        private class GalleryAsyncEnumerator : AbstractPixivAsyncEnumerator<Illustration>
        {
            private readonly RestrictPolicy restrictPolicy;
            private readonly string uid;
            private GalleryResponse entity;

            private IEnumerator<Illustration> illustrationsEnumerator;

            public GalleryAsyncEnumerator(string uid, IPixivAsyncEnumerable<Illustration> outerInstance, RestrictPolicy restrictPolicy) : base(outerInstance)
            {
                this.uid = uid;
                this.restrictPolicy = restrictPolicy;
            }

            public override Illustration Current => illustrationsEnumerator.Current;

            protected override void UpdateEnumerator()
            {
                illustrationsEnumerator = entity.Illusts.NonNull().Select(_ => _.Parse()).GetEnumerator();
            }

            public override async ValueTask<bool> MoveNextAsync()
            {
                if (entity == null)
                {
                    if (await TryGetResponse(restrictPolicy switch
                    {
                        RestrictPolicy.Public  => $"/v1/user/bookmarks/illust?user_id={uid}&restrict=public&filter=for_ios",
                        RestrictPolicy.Private => $"/v1/user/bookmarks/illust?user_id={uid}&restrict=private&filter=for_ios",
                        _                      => throw new ArgumentOutOfRangeException()
                    }) is (true, var model))
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

                if (illustrationsEnumerator.MoveNext())
                {
                    return true;
                }

                if (entity.NextUrl.IsNullOrEmpty())
                {
                    return false;
                }

                if (await TryGetResponse(entity.NextUrl) is (true, var response))
                {
                    entity = response;
                    UpdateEnumerator();
                    Enumerable.ReportRequestedPages();
                    return true;
                }

                return false;
            }

            private static async Task<HttpResponse<GalleryResponse>> TryGetResponse(string url)
            {
                var result = (await HttpClientFactory.AppApi.GetStringAsync(url)).FromJson<GalleryResponse>();

                if (result is { } response && !response.Illusts.IsNullOrEmpty())
                {
                    return HttpResponse<GalleryResponse>.Wrap(true, response);
                }
                return HttpResponse<GalleryResponse>.Wrap(false);
            }
        }
    }

    public class PublicGalleryAsyncEnumerable : AbstractGalleryAsyncEnumerable
    {
        public PublicGalleryAsyncEnumerable(string uid)
        {
            Uid = uid;
        }

        protected override string Uid { get; }

        protected override RestrictPolicy RestrictPolicy { get; } = RestrictPolicy.Public;
    }

    public class PrivateGalleryAsyncEnumerable : AbstractGalleryAsyncEnumerable
    {
        public PrivateGalleryAsyncEnumerable(string uid)
        {
            Uid = uid;
        }

        protected override string Uid { get; }

        protected override RestrictPolicy RestrictPolicy { get; } = RestrictPolicy.Private;
    }
}