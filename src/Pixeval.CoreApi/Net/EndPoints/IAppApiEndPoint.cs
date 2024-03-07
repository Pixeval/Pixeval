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
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Net.Response;
using Refit;

namespace Pixeval.CoreApi.Net.EndPoints;

internal interface IAppApiEndPoint
{
    [Post("/v2/illust/bookmark/add")]
    Task<HttpResponseMessage> AddIllustBookmarkAsync([Body(BodySerializationMethod.UrlEncoded)] AddBookmarkRequest request);

    [Post("/v1/illust/bookmark/delete")]
    Task<HttpResponseMessage> RemoveIllustBookmarkAsync([Body(BodySerializationMethod.UrlEncoded)] RemoveBookmarkRequest request);

    [Get("/v1/illust/detail")]
    Task<PixivSingleIllustResponse> GetSingleIllustAsync([AliasAs("illust_id")] long id);

    [Get("/v1/user/detail")]
    Task<PixivSingleUserResponse> GetSingleUserAsync(SingleUserRequest request);

    [Get("/v2/novel/detail")]
    Task<Novel> GetSingleNovelAsync([AliasAs("novel_id")] long id);

    [Get("/webview/v2/novel")]
    Task<string> GetNovelContentAsync([AliasAs("id")] long id
        //[AliasAs("viewer_version")] string viewerVersion = "20221031_ai",
        //[AliasAs("font")] string x1 = "mincho",
        //[AliasAs("font_size")] string x2 = "1.0em",
        //[AliasAs("line_height")] string x3 = "1.8",
        //[AliasAs("color")] string x4 = "#1F1F1F",
        //[AliasAs("background_color")] string x5 = "#FFFFFF",
        //[AliasAs("mode")] string x6 = "horizontal",
        //[AliasAs("theme")] string x7 = "light",
        //[AliasAs("margin_top")] string x8 = "60px",
        //[AliasAs("margin_bottom")] string x9 = "50px"
    );

    [Post("/v1/user/follow/add")]
    Task<HttpResponseMessage> FollowUserAsync([Body(BodySerializationMethod.UrlEncoded)] FollowUserRequest request);

    [Post("/v1/user/follow/delete")]
    Task<HttpResponseMessage> RemoveFollowUserAsync([Body(BodySerializationMethod.UrlEncoded)] RemoveFollowUserRequest request);

    [Get("/v1/trending-tags/illust")]
    Task<TrendingTagResponse> GetTrendingTagsAsync([AliasAs("filter")] string filter);

    [Get("/v1/trending-tags/novel")]
    Task<TrendingTagResponse> GetTrendingTagsForNovelAsync([AliasAs("filter")] string filter);

    [Get("/v1/ugoira/metadata")]
    Task<UgoiraMetadataResponse> GetUgoiraMetadataAsync([AliasAs("illust_id")] long id);

    [Post("/v1/illust/comment/delete")]
    Task DeleteIllustCommentAsync([Body(BodySerializationMethod.UrlEncoded)] DeleteCommentRequest request);

    [Get("/v2/search/autocomplete")]
    Task<AutoCompletionResponse> GetAutoCompletionAsync(AutoCompletionRequest autoCompletionRequest);
}
