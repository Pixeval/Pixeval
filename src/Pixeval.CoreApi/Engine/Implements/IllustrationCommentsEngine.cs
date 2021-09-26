using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;

namespace Pixeval.CoreApi.Engine.Implements
{
    public class IllustrationCommentsEngine : AbstractPixivFetchEngine<Comment>
    {
        private readonly string _illustId;

        public IllustrationCommentsEngine(string illustId, [NotNull] MakoClient makoClient, EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
            _illustId = illustId;
        }

        public override IAsyncEnumerator<Comment> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return RecursivePixivAsyncEnumerators.Comment<IllustrationCommentsEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
                engine => $"/v3/illust/comments?illust_id={engine._illustId}")!;
        }
    }
}