#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/IAppApiEndPoint.cs
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

using System.Net.Http;
using System.Threading.Tasks;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Net.Response;
using Refit;

namespace Pixeval.CoreApi.Net.EndPoints
{
    internal interface IAppApiEndPoint
    {
        [Post("/v2/illust/bookmark/add")]
        Task<HttpResponseMessage> AddBookmarkAsync([Body(BodySerializationMethod.UrlEncoded)] AddBookmarkRequest request);

        [Post("/v1/illust/bookmark/delete")]
        Task<HttpResponseMessage> RemoveBookmarkAsync([Body(BodySerializationMethod.UrlEncoded)] RemoveBookmarkRequest request);

        [Get("/v1/illust/detail")]
        Task<PixivSingleIllustResponse> GetSingleAsync([AliasAs("illust_id")] string id);

        [Get("/v1/user/detail")]
        Task<PixivSingleUserResponse> GetSingleUserAsync(SingleUserRequest request);

        [Post("/v1/user/follow/add")]
        Task<HttpResponseMessage> FollowUserAsync([Body(BodySerializationMethod.UrlEncoded)] FollowUserRequest request);

        [Post("/v1/user/follow/delete")]
        Task<HttpResponseMessage> RemoveFollowUserAsync([Body(BodySerializationMethod.UrlEncoded)] RemoveFollowUserRequest request);

        [Get("/v1/trending-tags/illust")]
        Task<TrendingTagResponse> GetTrendingTagsAsync([AliasAs("filter")] string filter);

        [Get("/v1/ugoira/metadata")]
        Task<UgoiraMetadataResponse> GetUgoiraMetadataAsync([AliasAs("illust_id")] string id);

        [Post("/v1/illust/comment/delete")]
        Task DeleteCommentAsync([Body(BodySerializationMethod.UrlEncoded)] DeleteCommentRequest request);
    }
}