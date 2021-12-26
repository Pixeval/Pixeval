﻿#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/RecommendationEngine.cs
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

internal class RecommendationEngine : AbstractPixivFetchEngine<Illustration>
{
    private readonly TargetFilter _filter;
    private readonly uint? _maxBookmarkIdForRecommend;
    private readonly uint? _minBookmarkIdForRecentIllust;

    public readonly RecommendationContentType _recommendContentType;

    public RecommendationEngine(MakoClient makoClient, RecommendationContentType? recommendContentType, TargetFilter filter, uint? maxBookmarkIdForRecommend, uint? minBookmarkIdForRecentIllust, EngineHandle? engineHandle) : base(makoClient, engineHandle)
    {
        _recommendContentType = recommendContentType ?? RecommendationContentType.Illust;
        _filter = filter;
        _maxBookmarkIdForRecommend = maxBookmarkIdForRecommend;
        _minBookmarkIdForRecentIllust = minBookmarkIdForRecentIllust;
    }

    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return RecursivePixivAsyncEnumerators.Illustration<RecommendationEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
            engine =>
            {
                var maxBookmarkIdForRecommend = engine._maxBookmarkIdForRecommend?.Let(static s => $"&max_bookmark_id_for_recommend={s}") ?? string.Empty;
                var maxBookmarkIdForRecentIllust = engine._minBookmarkIdForRecentIllust.Let(static s => $"&min_bookmark_id_for_recent_illust={s}") ?? string.Empty;
                return "/v1/illust/recommended"
                       + $"?filter={engine._filter.GetDescription()}"
                       + $"&content_type={engine._recommendContentType.GetDescription()}{maxBookmarkIdForRecommend}{maxBookmarkIdForRecentIllust}";
            })!;
    }
}