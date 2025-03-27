// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

internal class IllustrationNewEngine(
    MakoClient makoClient,
    WorkType contentType,
    TargetFilter filter,
    uint? maxIllustId,
    EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Illustration>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Illustration<IllustrationNewEngine>(this,
            "/v1/illust/new"
            + $"?content_type={contentType.GetDescription()}"
            + $"&filter={filter.GetDescription()}"
            + maxIllustId?.Let(static s => $"&max_illust_id={s}"));
}
