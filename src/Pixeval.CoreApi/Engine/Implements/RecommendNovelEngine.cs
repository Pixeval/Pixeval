#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2024 Pixeval.CoreApi/RecommendNovelEngine.cs
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

using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

internal class RecommendNovelEngine(
    MakoClient makoClient,
    TargetFilter filter,
    uint? maxBookmarkIdForRecommend,
    EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Novel>(makoClient, engineHandle)
{
    private readonly TargetFilter _filter = filter;
    private readonly uint? _maxBookmarkIdForRecommend = maxBookmarkIdForRecommend;

    public override IAsyncEnumerator<Novel> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken()) =>
        RecursivePixivAsyncEnumerators.Novel<RecommendNovelEngine>.WithInitialUrl(this,
            MakoApiKind.AppApi,
            engine =>
            {
                var maxBookmarkIdForRecommend =
                    engine._maxBookmarkIdForRecommend?.Let(static s => $"&max_bookmark_id_for_recommend={s}");
                return "/v1/novel/recommended"
                       + $"?filter={engine._filter.GetDescription()}"
                       + maxBookmarkIdForRecommend;
            })!;
}
