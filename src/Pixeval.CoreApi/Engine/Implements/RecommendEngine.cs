#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/RecommendEngine.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engine.Implements
{
    internal class RecommendEngine : AbstractPixivFetchEngine<Illustration>
    {
        private readonly TargetFilter _filter;
        private readonly uint? _maxBookmarkIdForRecommend;
        private readonly uint? _minBookmarkIdForRecentIllust;

        private readonly RecommendContentType _recommendContentType;

        public RecommendEngine(MakoClient makoClient, RecommendContentType? recommendContentType, TargetFilter filter, uint? maxBookmarkIdForRecommend, uint? minBookmarkIdForRecentIllust, EngineHandle? engineHandle) : base(makoClient, engineHandle)
        {
            _recommendContentType = recommendContentType ?? RecommendContentType.Illust;
            _filter = filter;
            _maxBookmarkIdForRecommend = maxBookmarkIdForRecommend;
            _minBookmarkIdForRecentIllust = minBookmarkIdForRecentIllust;
        }

        public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return RecursivePixivAsyncEnumerators.Illustration<RecommendEngine>.WithInitialUrl(this, MakoApiKind.AppApi,
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
}