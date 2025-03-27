// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Threading;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Utilities;

namespace Mako.Engine.Implements;

/// <summary>
/// 
/// </summary>
/// <param name="makoClient"></param>
/// <param name="engineHandle"></param>
/// <param name="matchOption"></param>
/// <param name="tag"></param>
/// <param name="sortOption"></param>
/// <param name="searchDuration"></param>
/// <param name="targetFilter"></param>
/// <param name="startDate"></param>
/// <param name="endDate"></param>
/// <param name="mergePlainKeywordResults"></param>
/// <param name="includeTranslatedTagResults"></param>
/// <param name="aiType">false：过滤AI</param>
internal class NovelSearchEngine(
    MakoClient makoClient,
    EngineHandle? engineHandle,
    SearchNovelTagMatchOption matchOption,
    string tag,
    WorkSortOption sortOption,
    TargetFilter targetFilter,
    DateTimeOffset? startDate,
    DateTimeOffset? endDate,
    bool mergePlainKeywordResults,
    bool includeTranslatedTagResults,
    bool? aiType)
    : AbstractPixivFetchEngine<Novel>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Novel> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return new RecursivePixivAsyncEnumerators.Novel<NovelSearchEngine>(
            this,
            "/v1/search/novel"
            + $"?search_target={matchOption.GetDescription()}"
            + $"&word={tag}"
            + $"&filter={targetFilter.GetDescription()}"
            + $"&merge_plain_keyword_results={mergePlainKeywordResults.ToString().ToLower()}"
            + $"&include_translated_tag_results={includeTranslatedTagResults.ToString().ToLower()}"
            + (sortOption is WorkSortOption.DoNotSort
                ? null
                : $"&sort={sortOption.GetDescription()}")
            + startDate?.Let(dn => $"&start_date={dn:yyyy-MM-dd}")
            + endDate?.Let(dn => $"&end_date={dn:yyyy-MM-dd}")
            + aiType?.Let(t => $"&search_ai_type={(t ? 1 : 0)}"));
    }
}
