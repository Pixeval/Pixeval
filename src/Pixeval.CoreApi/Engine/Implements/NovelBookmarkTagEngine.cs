// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

internal class NovelBookmarkTagEngine(MakoClient makoClient, long uid, PrivacyPolicy privacyPolicy, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<BookmarkTag>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<BookmarkTag> GetAsyncEnumerator(CancellationToken cancellationToken = new()) =>
        new RecursivePixivAsyncEnumerators.BookmarkTag<NovelBookmarkTagEngine>(
            this,
            "/v1/user/bookmark-tags/novel"
            + $"?user_id={uid}"
            + $"&restrict={privacyPolicy.GetDescription()}");
}
