using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines.Implements
{
    internal class UserFollowingEngine : AbstractPixivFetchEngine<User>
    {
        private readonly PrivacyPolicy _privacyPolicy;
        private readonly string _uid;
        
        public UserFollowingEngine([NotNull] MakoClient makoClient, PrivacyPolicy privacyPolicy, string uid, EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
            _privacyPolicy = privacyPolicy;
            _uid = uid;
        }

        public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return RecursivePixivAsyncEnumerators.User<UserFollowingEngine>.WithInitialUrl(this, MakoApiKind.AppApi, engine => $"/v1/user/following?user_id={engine._uid}&restrict={engine._privacyPolicy.GetDescription()}")!;
        }
    }
}