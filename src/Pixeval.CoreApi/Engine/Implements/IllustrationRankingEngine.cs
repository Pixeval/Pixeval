// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

internal class IllustrationRankingEngine(
    MakoClient makoClient,
    RankOption rankOption,
    DateTime dateTime,
    TargetFilter targetFilter,
    EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Illustration>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Illustration<IllustrationRankingEngine>(
            this,
            $"/v1/illust/ranking" +
            $"?filter={targetFilter.GetDescription()}" +
            $"&mode={rankOption.GetDescription()}" +
            $"&date={dateTime:yyyy-MM-dd}");
}
