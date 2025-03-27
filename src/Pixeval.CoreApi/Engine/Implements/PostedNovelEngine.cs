// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

public class PostedNovelEngine(MakoClient makoClient, long uid, TargetFilter targetFilter,
    EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Novel>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Novel> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Novel<PostedNovelEngine>(this,
            "/v1/user/novels" +
            $"?user_id={uid}" +
            $"&filter={targetFilter.GetDescription()}");
}
