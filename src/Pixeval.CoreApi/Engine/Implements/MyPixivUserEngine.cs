// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Engine.Implements;

internal class MyPixivUserEngine(MakoClient makoClient, long userId, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<User>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<User> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.User<MyPixivUserEngine>(
            this, 
            "/v1/user/mypixiv" + 
            $"?user_id={userId}");
}
