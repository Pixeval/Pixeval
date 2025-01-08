// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

internal class PostedIllustrationEngine(MakoClient makoClient, long uid, WorkType type, TargetFilter targetFilter, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Illustration>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Illustration<PostedIllustrationEngine>(
            this,
            "/v1/user/illusts"
            + $"?user_id={uid}"
            + $"&filter={targetFilter.GetDescription()}"
            + $"&type={type.GetDescription()}");
}
