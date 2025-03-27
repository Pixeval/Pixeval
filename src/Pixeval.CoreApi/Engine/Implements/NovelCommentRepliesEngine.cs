// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Model;

namespace Mako.Engine.Implements;

public class NovelCommentRepliesEngine(string commentId, MakoClient makoClient, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Comment>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Comment> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Comment<NovelCommentRepliesEngine>(
            this,
            $"/v2/novel/comment/replies" +
            $"?comment_id={commentId}");
}
