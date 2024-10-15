#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/SearchEngine.cs
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
/// <param name="aiType">false：过滤AI</param>
internal class IllustrationSearchEngine(
    MakoClient makoClient,
    EngineHandle? engineHandle,
    SearchIllustrationTagMatchOption matchOption,
    string tag,
    WorkSortOption sortOption,
    SearchDuration searchDuration,
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
            + endDate?.Let(dn => $"&start_date={dn:yyyy-MM-dd}")
            + (searchDuration is SearchDuration.Undecided
                ? null
                : $"&duration={searchDuration.GetDescription()}")
            + aiType?.Let(t => $"&search_ai_type={(t ? 1 : 0)}"));
    }
}
