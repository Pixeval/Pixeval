using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;

namespace Pixeval.CoreApi.Engine.Implements
{
    public class IllustrationCommentRepliesEngine : AbstractPixivFetchEngine<Comment>
    {
        private readonly string _commentId;

        public IllustrationCommentRepliesEngine(string commentId, [NotNull] MakoClient makoClient, EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
            _commentId = commentId;
        }

        public override IAsyncEnumerator<Comment> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return RecursivePixivAsyncEnumerators.Comment<IllustrationCommentRepliesEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
                engine => $"/v2/illust/comment/replies?comment_id={engine._commentId}")!;
        }
    }
}