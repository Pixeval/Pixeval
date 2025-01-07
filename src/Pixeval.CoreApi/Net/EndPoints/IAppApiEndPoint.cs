#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/IAppApiEndPoint.cs
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

using System.Net.Http;
using System.Threading.Tasks;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Net.Response;
using WebApiClientCore;
using WebApiClientCore.Attributes;

namespace Pixeval.CoreApi.Net.EndPoints;

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

    [HttpGet("/v1/illust/detail")]
    Task<PixivSingleIllustResponse> GetSingleIllustAsync([AliasAs("illust_id")] long id);

    [HttpGet("/v1/user/detail")]
    Task<PixivSingleUserResponse> GetSingleUserAsync(SingleUserRequest request);

    [HttpGet("/v2/novel/detail")]
    Task<PixivSingleNovelResponse> GetSingleNovelAsync([AliasAs("novel_id")] long id);

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

    [LoggingFilter]
    [HttpPost("/v1/user/follow/add")]
    Task<HttpResponseMessage> FollowUserAsync([FormContent] FollowUserRequest request);

    [HttpPost("/v1/user/follow/delete")]
    Task<HttpResponseMessage> RemoveFollowUserAsync([FormContent] RemoveFollowUserRequest request);

    [HttpGet("/v1/trending-tags/illust")]
    Task<TrendingTagResponse> GetTrendingTagsAsync(string filter);

    [HttpGet("/v1/trending-tags/novel")]
    Task<TrendingTagResponse> GetTrendingTagsForNovelAsync(string filter);

    [HttpGet("/v1/ugoira/metadata")]
    Task<UgoiraMetadataResponse> GetUgoiraMetadataAsync([AliasAs("illust_id")] long id);

    [HttpGet("/v2/search/autocomplete")]
    Task<AutoCompletionResponse> GetAutoCompletionAsync(AutoCompletionRequest autoCompletionRequest);

    [HttpPost("/v1/illust/comment/add")]
    Task<HttpResponseMessage> AddIllustCommentAsync([FormContent] AddNormalIllustCommentRequest request);

    [HttpPost("/v1/illust/comment/add")]
    Task<HttpResponseMessage> AddIllustCommentAsync([FormContent] AddStampIllustCommentRequest request);

    [HttpPost("/v1/illust/comment/delete")]
    Task<HttpResponseMessage> DeleteIllustCommentAsync([FormContent] DeleteCommentRequest request);

    [HttpPost("/v1/novel/comment/add")]
    Task<HttpResponseMessage> AddNovelCommentAsync([FormContent] AddNormalNovelCommentRequest request);

    [HttpPost("/v1/novel/comment/add")]
    Task<HttpResponseMessage> AddNovelCommentAsync([FormContent] AddStampNovelCommentRequest request);

    [HttpPost("/v1/novel/comment/delete")]
    Task<HttpResponseMessage> DeleteNovelCommentAsync([FormContent] DeleteCommentRequest request);
}
