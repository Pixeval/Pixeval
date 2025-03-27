// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Model;

namespace Mako.Engine.Implements;

public class NovelCommentsEngine(long novelId, MakoClient makoClient, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Comment>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Comment> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Comment<NovelCommentsEngine>(
            this,
            "/v3/novel/comments" +
            $"?novel_id={novelId}");
}
