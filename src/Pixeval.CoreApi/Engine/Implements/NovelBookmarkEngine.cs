// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using System.Web;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

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
