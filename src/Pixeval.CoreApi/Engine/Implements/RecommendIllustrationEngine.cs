// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Engine.Implements;

internal class RecommendIllustrationEngine(
    MakoClient makoClient,
    WorkType? contentType,
    TargetFilter filter,
    uint? maxBookmarkIdForRecommend,
    uint? minBookmarkIdForRecentIllust,
    EngineHandle? engineHandle)
    : AbstractPixivFetchEngine<Illustration>(makoClient, engineHandle)
{
    public override IAsyncEnumerator<Illustration> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) =>
        new RecursivePixivAsyncEnumerators.Illustration<RecommendIllustrationEngine>(this,
            "/v1/illust/recommended"
            + $"?filter={filter.GetDescription()}"
            + contentType?.Let(static s => $"&content_type={s.GetDescription()}")
            + maxBookmarkIdForRecommend?.Let(static s => $"&max_bookmark_id_for_recommend={s}")
            + minBookmarkIdForRecentIllust?.Let(static s => $"&min_bookmark_id_for_recent_illust={s}"));
}
