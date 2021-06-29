using System.Collections.Generic;
using System.Threading;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi.Engines.Implements
{
    internal class RecommendEngine : AbstractPixivFetchEngine<Illustration>
    {

        private readonly RecommendContentType _recommendContentType;
        private readonly TargetFilter _filter;
        private readonly uint? _maxBookmarkIdForRecommend;
        private readonly uint? _minBookmarkIdForRecentIllust;

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
                    return $"/v1/illust/recommended?filter={engine._filter.GetDescription()}&content_type={engine._recommendContentType.GetDescription()}{maxBookmarkIdForRecommend}{maxBookmarkIdForRecentIllust}";
                })!;
        }
    }
}