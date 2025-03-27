// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using System.Web;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

public class NovelBookmarkEngine(
    MakoClient makoClient,
    long uid,
    string? tag,
    PrivacyPolicy privacyPolicy,
    TargetFilter targetFilter,
    EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Novel>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Novel> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Novel<NovelBookmarkEngine>(
            this,
            "/v1/user/bookmarks/novel"
            + $"?user_id={uid}"
            + $"&restrict={privacyPolicy.GetDescription()}"
            + $"&filter={targetFilter.GetDescription()}"
            + tag?.Let(s => $"&tag={HttpUtility.UrlEncode(s)}"));
}
