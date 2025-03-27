// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

internal class NovelNewEngine(
    MakoClient makoClient,
    TargetFilter filter,
    uint? maxNovelId,
    EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Novel>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Novel> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Novel<NovelNewEngine>(this,
            "/v1/novel/new"
            + $"?filter={filter.GetDescription()}"
            + maxNovelId?.Let(static s => $"&max_novel_id={s}"));
}
