// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

/// <summary>
/// 
/// </summary>
/// <param name="makoClient"></param>
/// <param name="engineHandle"></param>
/// <param name="matchOption"></param>
/// <param name="tag"></param>
/// <param name="sortOption"></param>
/// <param name="targetFilter"></param>
/// <param name="startDate"></param>
/// <param name="endDate"></param>
/// <param name="aiType">false：过滤AI</param>
internal class IllustrationSearchEngine(
    MakoClient makoClient,
    EngineHandle? engineHandle,
    SearchIllustrationTagMatchOption matchOption,
    string tag,
    WorkSortOption sortOption,
    TargetFilter targetFilter,
    DateTimeOffset? startDate,
    DateTimeOffset? endDate,
    bool? aiType)
    : AbstractPixivFetchEngine<Illustration>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return new RecursivePixivAsyncEnumerators.Illustration<IllustrationSearchEngine>(
            this,
            "/v1/search/illust"
            + $"?search_target={matchOption.GetDescription()}"
            + $"&word={tag}"
            + $"&filter={targetFilter.GetDescription()}"
            + (sortOption is WorkSortOption.DoNotSort
                ? null
                : $"&sort={sortOption.GetDescription()}")
            + startDate?.Let(dn => $"&start_date={dn:yyyy-MM-dd}")
            + endDate?.Let(dn => $"&end_date={dn:yyyy-MM-dd}")
            + aiType?.Let(t => $"&search_ai_type={(t ? 1 : 0)}"));
    }
}
