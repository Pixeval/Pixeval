// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Net.Http;
using System.Threading.Tasks;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Net.Response;
using WebApiClientCore;
using WebApiClientCore.Attributes;

namespace Pixeval.CoreApi.Net.EndPoints;

/// <summary>
/// 方法上 [LoggingFilter] 输出日志
/// </summary>
[HttpHost(MakoHttpOptions.AppApiBaseUrl)]
public interface IAppApiEndPoint
{
    [HttpPost("/v2/illust/bookmark/add")]
    Task<HttpResponseMessage> AddIllustBookmarkAsync([FormContent] AddIllustBookmarkRequest request);

    [HttpPost("/v1/illust/bookmark/delete")]
    Task<HttpResponseMessage> RemoveIllustBookmarkAsync([FormContent] RemoveIllustBookmarkRequest request);

    [HttpPost("/v2/novel/bookmark/add")]
    Task<HttpResponseMessage> AddNovelBookmarkAsync([FormContent] AddNovelBookmarkRequest request);

    [HttpPost("/v1/novel/bookmark/delete")]
    Task<HttpResponseMessage> RemoveNovelBookmarkAsync([FormContent] RemoveNovelBookmarkRequest request);

    [Cache(60 * 1000)]
    [HttpGet("/v1/illust/detail")]
    Task<PixivSingleIllustResponse> GetSingleIllustAsync([AliasAs("illust_id")] long id);

    [Cache(60 * 1000)]
    [HttpGet("/v1/user/detail")]
    Task<PixivSingleUserResponse> GetSingleUserAsync([AliasAs("user_id")] long id, string filter);

    [Cache(60 * 1000)]
    [HttpGet("/v2/novel/detail")]
    Task<PixivSingleNovelResponse> GetSingleNovelAsync([AliasAs("novel_id")] long id);

    [Cache(60 * 1000)]
    [HttpGet("/webview/v2/novel")]
    Task<string> GetNovelContentAsync(long id, bool raw = false);
    /*
    [AliasAs("viewer_version")] string viewerVersion = "20221031_ai",
    [AliasAs("font")] string x1 = "mincho",
    [AliasAs("font_size")] string x2 = "1.0em",
    [AliasAs("line_height")] string x3 = "1.8",
    [AliasAs("color")] string x4 = "#1F1F1F",
    [AliasAs("background_color")] string x5 = "#FFFFFF",
    [AliasAs("mode")] string x6 = "horizontal",
    [AliasAs("theme")] string x7 = "light",
    [AliasAs("margin_top")] string x8 = "60px",
    [AliasAs("margin_bottom")] string x9 = "50px"
    */

    [HttpGet("/v1/user/related")]
    Task<PixivRelatedUsersResponse> RelatedUserAsync([AliasAs("seed_user_id")] long userId, string filter);

    [HttpPost("/v1/user/follow/add")]
    Task<HttpResponseMessage> FollowUserAsync([FormContent] FollowUserRequest request);

    [HttpPost("/v1/user/follow/delete")]
    Task<HttpResponseMessage> RemoveFollowUserAsync([FormContent] RemoveFollowUserRequest request);

    [HttpGet("/v1/trending-tags/illust")]
    Task<TrendingTagResponse> GetTrendingTagsAsync(string filter);

    [HttpGet("/v1/trending-tags/novel")]
    Task<TrendingTagResponse> GetTrendingTagsForNovelAsync(string filter);

    [Cache(60 * 1000)]
    [HttpGet("/v1/ugoira/metadata")]
    Task<UgoiraMetadataResponse> GetUgoiraMetadataAsync([AliasAs("illust_id")] long id);

    [HttpGet("/v2/search/autocomplete")]
    Task<AutoCompletionResponse> GetAutoCompletionAsync(string word, [AliasAs("merge_plain_keyword_results")] bool mergePlainKeywordResult = true);

    [HttpPost("/v1/illust/comment/add")]
    Task<PostCommentResponse> AddIllustCommentAsync([FormContent] AddNormalIllustCommentRequest request);

    [HttpPost("/v1/illust/comment/add")]
    Task<PostCommentResponse> AddIllustCommentAsync([FormContent] AddStampIllustCommentRequest request);

    [HttpPost("/v1/illust/comment/delete")]
    Task<HttpResponseMessage> DeleteIllustCommentAsync([FormContent] DeleteCommentRequest request);

    [HttpPost("/v1/novel/comment/add")]
    Task<PostCommentResponse> AddNovelCommentAsync([FormContent] AddNormalNovelCommentRequest request);

    [HttpPost("/v1/novel/comment/add")]
    Task<PostCommentResponse> AddNovelCommentAsync([FormContent] AddStampNovelCommentRequest request);

    [HttpPost("/v1/novel/comment/delete")]
    Task<HttpResponseMessage> DeleteNovelCommentAsync([FormContent] DeleteCommentRequest request);

    [HttpGet("/v1/user/ai-show-settings")]
    Task<ShowAiSettingsResponse> GetAiShowSettingsAsync();

    [HttpPost("/v1/user/ai-show-settings/edit")]
    Task<HttpResponseMessage> PostAiShowSettingsAsync([FormContent] ShowAiSettingsRequest request);

    [HttpGet("/v1/user/restricted-mode-settings")]
    Task<RestrictedModeSettingsResponse> GetRestrictedModeSettingsAsync();

    [HttpPost("/v1/user/restricted-mode-settings")]
    Task<HttpResponseMessage> PostRestrictedModeSettingsAsync([FormContent] RestrictedModeSettingsRequest request);
}
