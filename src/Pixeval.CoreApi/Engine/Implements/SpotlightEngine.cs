// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Mako.Model;

namespace Mako.Engine.Implements;

internal class SpotlightEngine(MakoClient makoClient, EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Spotlight>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Spotlight> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Spotlight<SpotlightEngine>(
            this,
            "/v1/spotlight/articles");
}
