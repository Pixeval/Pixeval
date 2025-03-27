// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

public class RecentPostedIllustrationEngine(MakoClient makoClient, PrivacyPolicy privacyPolicy, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Illustration>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Illustration<RecentPostedIllustrationEngine>(
            this,
            "/v2/illust/follow" +
            $"?restrict={privacyPolicy.GetDescription()}");
}
