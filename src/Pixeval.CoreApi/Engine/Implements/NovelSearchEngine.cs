#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2024 Pixeval.CoreApi/NovelSearchEngine.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

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
    SearchDuration searchDuration,
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
            + endDate?.Let(dn => $"&start_date={dn:yyyy-MM-dd}")
            + (searchDuration is SearchDuration.Undecided
                ? null
                : $"&duration={searchDuration.GetDescription()}")
            + aiType?.Let(t => $"&search_ai_type={(t ? 1 : 0)}"));
    }
}
