// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

internal class FollowingEngine(MakoClient makoClient, long uid, PrivacyPolicy privacyPolicy, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<User>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = new()) =>
        new RecursivePixivAsyncEnumerators.User<FollowingEngine>(
            this,
            "/v1/user/following" +
            $"?user_id={uid}" +
            $"&restrict={privacyPolicy.GetDescription()}");
}
