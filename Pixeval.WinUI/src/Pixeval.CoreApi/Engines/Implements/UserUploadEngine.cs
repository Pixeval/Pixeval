using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;

namespace Pixeval.CoreApi.Engines.Implements
{
    internal class UserUploadEngine : AbstractPixivFetchEngine<Illustration>
    {
        private readonly string _uid;
        
        public UserUploadEngine([NotNull] MakoClient makoClient, string uid, EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
            _uid = uid;
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return RecursivePixivAsyncEnumerators.Illustration<UserUploadEngine>.WithInitialUrl(this, MakoApiKind.AppApi, engine => $"/v1/user/illusts?user_id={engine._uid}&filter=for_android&type=illust")!;
        }
    }
}