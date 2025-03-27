// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

internal class RecommendIllustratorEngine(MakoClient makoClient, TargetFilter targetFilter, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<User>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.User<RecommendIllustratorEngine>(
            this,
            "/v1/user/recommended" +
            $"?filter={targetFilter.GetDescription()}");
}
