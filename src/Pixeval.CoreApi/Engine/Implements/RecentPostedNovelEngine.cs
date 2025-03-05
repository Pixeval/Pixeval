// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

public class RecentPostedNovelEngine(MakoClient makoClient, PrivacyPolicy privacyPolicy,
    EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Novel>(makoClient, engineHandle)
{

    public override IAsyncEnumerator<Novel> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Novel<RecentPostedNovelEngine>(
            this,
            "/v1/novel/follow"
            + $"?restrict={privacyPolicy.GetDescription()}");
}
