// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

public class UserSearchEngine(MakoClient makoClient, TargetFilter targetFilter,
        string keyword, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<User>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.User<UserSearchEngine>(
            this,
            "/v1/search/user" +
            $"?filter={targetFilter.GetDescription()}" +
            $"&word={keyword}");
}
