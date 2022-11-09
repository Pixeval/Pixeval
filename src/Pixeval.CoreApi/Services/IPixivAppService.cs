using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pixeval.CoreApi.Enums;
using Pixeval.CoreApi.Net.Requests;
using Pixeval.CoreApi.Net.Responses;

namespace Pixeval.CoreApi.Services
{
    public interface IPixivAppService
    {
        [Get("/v1/user/me/state")]
        Task<UserMeStateResponse> GetUserMeStateAsync();

        [Get("/v1/user/detail?filter=for_android")]
        Task<PixivSingleUserResponse> GetUserDetailAsync([AliasAs("user_id")] long id);

        [Get("/v1/illust/recommended?filter=for_android")]
        Task<RecommendationsResponse> GetRecommendedIllustrationsAsync([AliasAs("include_ranking_illusts")] bool? includeRankingIllustrations = null, [AliasAs("include_privacy_policy")] bool? includePrivacyPolicy = null);

        [Get("/v1/illust/detail?filter=for_android")]
        Task<PixivSingleIllustResponse> GetIllustrationDetailAsync([AliasAs("illust_id")] long illustrationId);

        [Post("/v2/illust/bookmark/add")]
        Task<HttpResponseMessage> AddBookmarkAsync([Body(BodySerializationMethod.UrlEncoded)] AddBookmarkRequest request);

        [Post("/v1/illust/bookmark/delete")]
        Task<HttpResponseMessage> RemoveBookmarkAsync([Body(BodySerializationMethod.UrlEncoded)] RemoveBookmarkRequest request);

        [Post("/v1/user/follow/add")]
        Task<HttpResponseMessage> FollowUserAsync([Body(BodySerializationMethod.UrlEncoded)] FollowUserRequest request);

        [Post("/v1/user/follow/delete")]
        Task<HttpResponseMessage> RemoveFollowUserAsync([Body(BodySerializationMethod.UrlEncoded)] RemoveFollowUserRequest request);

        [Get("/v1/trending-tags/illust")]
        Task<TrendingTagResponse> GetTrendingTagsAsync([AliasAs("filter")] string filter);

        [Get("/v1/trending-tags/novel")]
        Task<TrendingTagResponse> GetTrendingTagsForNovelAsync([AliasAs("filter")] string filter);

        [Get("/v1/ugoira/metadata")]
        Task<UgoiraMetadataResponse> GetUgoiraMetadataAsync([AliasAs("illust_id")] string id);

        [Post("/v1/illust/comment/delete")]
        Task DeleteCommentAsync([Body(BodySerializationMethod.UrlEncoded)] DeleteCommentRequest request);

        [Get("/v2/search/autocomplete")]
        Task<AutoCompletionResponse> GetAutoCompletionAsync(AutoCompletionRequest autoCompletionRequest);
    }
}
