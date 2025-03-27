// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Model;

namespace Mako.Engine.Implements;

public class IllustrationCommentsEngine(long illustId, MakoClient makoClient, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Comment>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Comment> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Comment<IllustrationCommentsEngine>(
            this,
            $"/v3/illust/comments" +
            $"?illust_id={illustId}");
}
