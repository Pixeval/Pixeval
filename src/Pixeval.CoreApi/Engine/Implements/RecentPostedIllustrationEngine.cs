// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

public class RecentPostedIllustrationEngine(MakoClient makoClient, PrivacyPolicy privacyPolicy, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Illustration>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Illustration<RecentPostedIllustrationEngine>(
            this,
            "/v2/illust/follow" +
            $"?restrict={privacyPolicy.GetDescription()}");
}
