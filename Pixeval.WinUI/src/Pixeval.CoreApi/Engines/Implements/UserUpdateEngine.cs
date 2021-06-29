using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines.Implements
{
    public class UserUpdateEngine : AbstractPixivFetchEngine<Illustration>
    {
        private readonly PrivacyPolicy _privacyPolicy;
        
        public UserUpdateEngine([NotNull] MakoClient makoClient, PrivacyPolicy privacyPolicy, EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
            _privacyPolicy = privacyPolicy;
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return RecursivePixivAsyncEnumerators.Illustration<UserUpdateEngine>.WithInitialUrl(this, MakoApiKind.AppApi, engine => $"/v2/illust/follow?restrict={engine._privacyPolicy.GetDescription()}")!;
        }
    }
}