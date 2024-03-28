#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/MakoClient.Extensions.cs
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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.EndPoints;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;
using Refit;

namespace Pixeval.CoreApi;

public partial class MakoClient
{
    /// <summary>
    /// Gets the detail of an illustration from the illust id
    /// </summary>
    /// <param name="id">The illust id</param>
    /// <returns></returns>
    public Task<Illustration> GetIllustrationFromIdAsync(long id)
        => RunWithLoggerAsync(async t => (await t
            .GetSingleIllustAsync(id)
            .ConfigureAwait(false)).Illust);

    public Task<Tag[]> GetAutoCompletionForKeyword(string word)
        => RunWithLoggerAsync(async t => (await t
            .GetAutoCompletionAsync(new AutoCompletionRequest(word))
            .ConfigureAwait(false))
            .Tags);

    public Task<PixivSingleUserResponse> GetUserFromIdAsync(long id, TargetFilter targetFilter)
        => RunWithLoggerAsync(async t => await t
            .GetSingleUserAsync(new SingleUserRequest(id, targetFilter.GetDescription()))
            .ConfigureAwait(false));

    public Task<Novel> GetNovelFromIdAsync(long id)
        => RunWithLoggerAsync(async t => (await t
            .GetSingleNovelAsync(id)
            .ConfigureAwait(false)).Novel);

    public Task<NovelContent> GetNovelContentAsync(long id)
        => RunWithLoggerAsync(async t =>
        {
            var contentHtml = await t
                .GetNovelContentAsync(id)
                .ConfigureAwait(false);

            var leftStack = -2;
            var rightStack = 0;
            var startIndex = -1;
            var endIndex = -1;

            for (var i = 0; i < contentHtml.Length; ++i)
            {
                if (contentHtml[i] is '{')
                {
                    ++leftStack;
                    if (leftStack < 2)
                        startIndex = i;
                }
                else if (contentHtml[i] is '}')
                {
                    ++rightStack;
                    if (rightStack == leftStack)
                    {
                        endIndex = i + 1;
                        break;
                    }
                }
            }

            var span = contentHtml[startIndex..endIndex];

            return JsonSerializer.Deserialize<NovelContent>(span)!;
        });

    /// <summary>
    /// Sends a request to the Pixiv to add it to the bookmark
    /// </summary>
    /// <param name="id">The ID of the illustration which needs to be bookmarked</param>
    /// <param name="privacyPolicy">Indicates the privacy of the illustration in the bookmark</param>
    /// <param name="tags"></param>
    /// <returns>A <see cref="Task" /> represents the operation</returns>
    public Task<HttpResponseMessage> PostIllustrationBookmarkAsync(long id, PrivacyPolicy privacyPolicy, IEnumerable<string>? tags = null) =>
        RunWithLoggerAsync(async t =>
        {
            var urlTags = tags is null ? null : string.Join(' ', tags);
            return await t
                .AddIllustBookmarkAsync(new AddIllustBookmarkRequest(privacyPolicy, id, urlTags))
                .ConfigureAwait(false);
        });

    /// <summary>
    /// Sends a request to the Pixiv to remove it from the bookmark
    /// </summary>
    /// <param name="id">The ID of the illustration which needs to be removed from the bookmark</param>
    /// <returns>A <see cref="Task" /> represents the operation</returns>
    public Task<HttpResponseMessage> RemoveIllustrationBookmarkAsync(long id)
        => RunWithLoggerAsync(async t => await t
            .RemoveIllustBookmarkAsync(new RemoveIllustBookmarkRequest(id))
            .ConfigureAwait(false));

    public Task<HttpResponseMessage> PostNovelBookmarkAsync(long id, PrivacyPolicy privacyPolicy, IEnumerable<string>? tags = null) =>
        RunWithLoggerAsync(async t =>
        {
            var urlTags = tags is null ? null : string.Join(' ', tags);
            return await t
                .AddNovelBookmarkAsync(new AddNovelBookmarkRequest(privacyPolicy, id, urlTags))
                .ConfigureAwait(false);
        });

    public Task<HttpResponseMessage> RemoveNovelBookmarkAsync(long id)
        => RunWithLoggerAsync(async t => await t
            .RemoveNovelBookmarkAsync(new RemoveNovelBookmarkRequest(id))
            .ConfigureAwait(false));

    public Task<PixivRelatedUsersResponse> RelatedUserAsync(long id, TargetFilter filter = TargetFilter.ForAndroid)
        => RunWithLoggerAsync(async t => await t
            .RelatedUserAsync(filter, id)
            .ConfigureAwait(false));

    public Task<HttpResponseMessage> PostFollowUserAsync(long id, PrivacyPolicy privacyPolicy)
        => RunWithLoggerAsync(async t => await t
            .FollowUserAsync(new FollowUserRequest(id, privacyPolicy))
            .ConfigureAwait(false));

    public Task<HttpResponseMessage> RemoveFollowUserAsync(long id)
        => RunWithLoggerAsync(async t => await t
            .RemoveFollowUserAsync(new RemoveFollowUserRequest(id))
            .ConfigureAwait(false));

    public Task<IEnumerable<TrendingTag>> GetTrendingTagsAsync(TargetFilter targetFilter)
        => RunWithLoggerAsync(async t => (await t
            .GetTrendingTagsAsync(targetFilter)
            .ConfigureAwait(false))
            .TrendTags
            .Select(tag => new TrendingTag(tag.TagStr, tag.TranslatedName, tag.Illust)));

    public Task<IEnumerable<TrendingTag>> GetTrendingTagsForNovelAsync(TargetFilter targetFilter)
        => RunWithLoggerAsync(async t => (await t
            .GetTrendingTagsForNovelAsync(targetFilter)
            .ConfigureAwait(false))
            .TrendTags
            .Select(tag => new TrendingTag(tag.TagStr, tag.TranslatedName, tag.Illust)));

    public Task<UgoiraMetadataResponse> GetUgoiraMetadataAsync(long id)
        => RunWithLoggerAsync(async t => await t
            .GetUgoiraMetadataAsync(id)
            .ConfigureAwait(false));

    public Task<HttpResponseMessage> DeleteIllustCommentAsync(long commentId)
        => RunWithLoggerAsync(async t => await t
            .DeleteIllustCommentAsync(new DeleteCommentRequest(commentId)));

    public Task<HttpResponseMessage> DeleteNovelCommentAsync(long commentId)
        => RunWithLoggerAsync(async t => await t
            .DeleteNovelCommentAsync(new DeleteCommentRequest(commentId)));

    public Task<HttpResponseMessage> AddIllustCommentAsync(long illustId, string content)
        => RunWithLoggerAsync(async t => await t
            .AddIllustCommentAsync(new AddNormalIllustCommentRequest(illustId, null, content)));

    public Task<HttpResponseMessage> AddIllustCommentAsync(long illustId, int stampId)
        => RunWithLoggerAsync(async t => await t
            .AddIllustCommentAsync(new AddStampIllustCommentRequest(illustId, null, stampId)));

    public Task<HttpResponseMessage> AddIllustCommentAsync(long illustId, long parentCommentId, string content)
        => RunWithLoggerAsync(async t => await t
            .AddIllustCommentAsync(new AddNormalIllustCommentRequest(illustId, parentCommentId, content)));

    public Task<HttpResponseMessage> AddIllustCommentAsync(long illustId, long parentCommentId, int stampId)
        => RunWithLoggerAsync(async t => await t
            .AddIllustCommentAsync(new AddStampIllustCommentRequest(illustId, parentCommentId, stampId)));

    public Task<HttpResponseMessage> AddNovelCommentAsync(long novelId, string content)
        => RunWithLoggerAsync(async t => await t
            .AddNovelCommentAsync(new AddNormalNovelCommentRequest(novelId, null, content)));

    public Task<HttpResponseMessage> AddNovelCommentAsync(long novelId, int stampId)
        => RunWithLoggerAsync(async t => await t
            .AddNovelCommentAsync(new AddStampNovelCommentRequest(novelId, null, stampId)));

    public Task<HttpResponseMessage> AddNovelCommentAsync(long novelId, long parentCommentId, string content)
        => RunWithLoggerAsync(async t => await t
            .AddNovelCommentAsync(new AddNormalNovelCommentRequest(novelId, parentCommentId, content)));

    public Task<HttpResponseMessage> AddNovelCommentAsync(long novelId, long parentCommentId, int stampId)
        => RunWithLoggerAsync(async t => await t
            .AddNovelCommentAsync(new AddStampNovelCommentRequest(novelId, parentCommentId, stampId)));

    public Task<ReverseSearchResponse> ReverseSearchAsync(Stream imgStream, string apiKey)
        => RunWithLoggerAsync(async () => await MakoServices.GetRequiredService<IReverseSearchApiEndPoint>()
            .GetSauceAsync(new ReverseSearchRequest(apiKey), new StreamPart(imgStream, "img")));
}
