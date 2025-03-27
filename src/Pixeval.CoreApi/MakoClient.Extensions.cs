// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mako.Global.Enum;
using Mako.Model;
using Mako.Net.EndPoints;
using Mako.Net.Request;
using Mako.Net.Response;
using Pixeval.Utilities;
using WebApiClientCore.Parameters;

namespace Mako;

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
            .GetAutoCompletionAsync(word)
            .ConfigureAwait(false))
            .Tags);

    public Task<PixivSingleUserResponse> GetUserFromIdAsync(long id, TargetFilter targetFilter)
        => RunWithLoggerAsync(async t => await t
            .GetSingleUserAsync(id, targetFilter.GetDescription())
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

            return (NovelContent) JsonSerializer.Deserialize(span, typeof(NovelContent), AppJsonSerializerContext.Default)!;
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

    public Task<User[]> RelatedUserAsync(long id, TargetFilter filter)
        => RunWithLoggerAsync(async t => (await t
                .RelatedUserAsync(id, filter.GetDescription())
                .ConfigureAwait(false))
            .Users);

    public Task<HttpResponseMessage> PostFollowUserAsync(long id, PrivacyPolicy privacyPolicy)
        => RunWithLoggerAsync(async t => await t
            .FollowUserAsync(new FollowUserRequest(id, privacyPolicy))
            .ConfigureAwait(false));

    public Task<HttpResponseMessage> RemoveFollowUserAsync(long id)
        => RunWithLoggerAsync(async t => await t
            .RemoveFollowUserAsync(new RemoveFollowUserRequest(id))
            .ConfigureAwait(false));

    public Task<TrendingTag[]> GetTrendingTagsAsync(TargetFilter targetFilter)
        => RunWithLoggerAsync(async t => (await t
                .GetTrendingTagsAsync(targetFilter.GetDescription())
                .ConfigureAwait(false))
            .TrendTags);

    public Task<TrendingTag[]> GetTrendingTagsForNovelAsync(TargetFilter targetFilter)
        => RunWithLoggerAsync(async t => (await t
                .GetTrendingTagsForNovelAsync(targetFilter.GetDescription())
                .ConfigureAwait(false))
            .TrendTags);

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

    public Task<Comment> AddIllustCommentAsync(long illustId, string content)
        => RunWithLoggerAsync(async t => (await t
            .AddIllustCommentAsync(new AddNormalIllustCommentRequest(illustId, null, content))).Comment);

    public Task<Comment> AddIllustCommentAsync(long illustId, int stampId)
        => RunWithLoggerAsync(async t => (await t
            .AddIllustCommentAsync(new AddStampIllustCommentRequest(illustId, null, stampId))).Comment);

    public Task<Comment> AddIllustCommentAsync(long illustId, long parentCommentId, string content)
        => RunWithLoggerAsync(async t => (await t
            .AddIllustCommentAsync(new AddNormalIllustCommentRequest(illustId, parentCommentId, content))).Comment);

    public Task<Comment> AddIllustCommentAsync(long illustId, long parentCommentId, int stampId)
        => RunWithLoggerAsync(async t => (await t
            .AddIllustCommentAsync(new AddStampIllustCommentRequest(illustId, parentCommentId, stampId))).Comment);

    public Task<Comment> AddNovelCommentAsync(long novelId, string content)
        => RunWithLoggerAsync(async t => (await t
            .AddNovelCommentAsync(new AddNormalNovelCommentRequest(novelId, null, content))).Comment);

    public Task<Comment> AddNovelCommentAsync(long novelId, int stampId)
        => RunWithLoggerAsync(async t => (await t
            .AddNovelCommentAsync(new AddStampNovelCommentRequest(novelId, null, stampId))).Comment);

    public Task<Comment> AddNovelCommentAsync(long novelId, long parentCommentId, string content)
        => RunWithLoggerAsync(async t => (await t
            .AddNovelCommentAsync(new AddNormalNovelCommentRequest(novelId, parentCommentId, content))).Comment);

    public Task<Comment> AddNovelCommentAsync(long novelId, long parentCommentId, int stampId)
        => RunWithLoggerAsync(async t => (await t
            .AddNovelCommentAsync(new AddStampNovelCommentRequest(novelId, parentCommentId, stampId))).Comment);

    public Task<bool> GetAiShowSettingsAsync()
        => RunWithLoggerAsync(async t => (await t.GetAiShowSettingsAsync()).ShowAi);

    public Task<HttpResponseMessage> PostAiShowSettingsAsync(bool showAi)
        => RunWithLoggerAsync(async t => await t.PostAiShowSettingsAsync(new ShowAiSettingsRequest(showAi)));

    public Task<bool> GetRestrictedModeSettingsAsync()
        => RunWithLoggerAsync(async t => (await t.GetRestrictedModeSettingsAsync()).IsRestrictedModeEnabled);

    public Task<HttpResponseMessage> PostRestrictedModeSettingsAsync(bool isRestrictedModeEnabled)
        => RunWithLoggerAsync(async t => await t.PostRestrictedModeSettingsAsync(new RestrictedModeSettingsRequest(isRestrictedModeEnabled)));

    public Task<ReverseSearchResponse> ReverseSearchAsync(Stream imgStream, string apiKey)
        => RunWithLoggerAsync(async () => await Provider.GetRequiredService<IReverseSearchApiEndPoint>()
            .GetSauceAsync(new FormDataFile(imgStream, "img"), new ReverseSearchRequest(apiKey)));
}
