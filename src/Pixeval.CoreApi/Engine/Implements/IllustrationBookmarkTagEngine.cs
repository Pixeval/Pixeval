// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

internal class IllustrationBookmarkTagEngine(MakoClient makoClient, long uid, PrivacyPolicy privacyPolicy, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<BookmarkTag>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<BookmarkTag> GetAsyncEnumerator(CancellationToken cancellationToken = new()) =>
        new RecursivePixivAsyncEnumerators.BookmarkTag<IllustrationBookmarkTagEngine>(
            this,
            "/v1/user/bookmark-tags/illust"
            + $"?user_id={uid}"
            + $"&restrict={privacyPolicy.GetDescription()}");
}
