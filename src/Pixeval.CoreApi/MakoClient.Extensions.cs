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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Global.Exception;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.EndPoints;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;
using Refit;

namespace Pixeval.CoreApi;

public partial class MakoClient
{
    public Task<PixivRelatedRecommendUsersResponse> GetRelatedRecommendUsersAsync(long uid, int num = 20, int workNum = 3, bool isR18 = true, CultureInfo? lang = null)
        => RunWithLoggerAsync(async () =>
        {
            var culture = lang ?? CultureInfo.CurrentCulture;
            return (await GetMakoHttpClient(MakoApiKind.WebApi)
                .GetStringResultAsync(
                    $"/ajax/user/{uid}/recommends?userNum={num}&workNum={workNum}&isR18={isR18.ToString().ToLower()}&lang={culture.TwoLetterISOLanguageName}",
                    message => MakoNetworkException.FromHttpResponseMessageAsync(message, Configuration.Bypass))
                .ConfigureAwait(false)).Rewrap(t => t.FromJson<PixivRelatedRecommendUsersResponse>()!);
        });

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
    /// <returns>A <see cref="Task" /> represents the operation</returns>
    public Task<HttpResponseMessage> PostIllustrationBookmarkAsync(long id, PrivacyPolicy privacyPolicy) =>
        RunWithLoggerAsync(async t => await t
            .AddIllustBookmarkAsync(new AddIllustBookmarkRequest(privacyPolicy.GetDescription(), id.ToString()))
            .ConfigureAwait(false));

    /// <summary>
    /// Sends a request to the Pixiv to remove it from the bookmark
    /// </summary>
    /// <param name="id">The ID of the illustration which needs to be removed from the bookmark</param>
    /// <returns>A <see cref="Task" /> represents the operation</returns>
    public Task<HttpResponseMessage> RemoveIllustrationBookmarkAsync(long id)
        => RunWithLoggerAsync(async t => await t
            .RemoveIllustBookmarkAsync(new RemoveIllustBookmarkRequest(id.ToString()))
            .ConfigureAwait(false));

    public Task<HttpResponseMessage> PostNovelBookmarkAsync(long id, PrivacyPolicy privacyPolicy) =>
        RunWithLoggerAsync(async t => await t
            .AddNovelBookmarkAsync(new AddNovelBookmarkRequest(privacyPolicy.GetDescription(), id.ToString()))
            .ConfigureAwait(false));

    public Task<HttpResponseMessage> RemoveNovelBookmarkAsync(long id)
        => RunWithLoggerAsync(async t => await t
            .RemoveNovelBookmarkAsync(new RemoveNovelBookmarkRequest(id.ToString()))
            .ConfigureAwait(false));

    /// <summary>
    /// Gets the details of a spotlight from its ID which contains the article information, introduction, and illustrations
    /// </summary>
    /// <param name="spotlightId">The ID of the spotlight</param>
    /// <returns>A <see cref="Task{TResult}" /> contains the result of the operation</returns>
    public async Task<SpotlightDetail?> GetSpotlightDetailAsync(long spotlightId)
    {
        EnsureNotCancelled();
        var result = (await GetMakoHttpClient(MakoApiKind.WebApi)
                .GetStringResultAsync($"/ajax/showcase/article?article_id={spotlightId}",
                    message => MakoNetworkException.FromHttpResponseMessageAsync(message, Configuration.Bypass))
                .ConfigureAwait(false))
            .UnwrapOrThrow().FromJson<PixivSpotlightDetailResponse>();
        if (result?.ResponseBody is null)
        {
            return null;
        }

        var illustrations = await result.ResponseBody[0].Illusts.SelectNotNull(illust => GetIllustrationFromIdAsync(illust.IllustId)).WhenAll().ConfigureAwait(false);
        foreach (var illustration in illustrations)
        {
            illustration.FromSpotlight = true;
            illustration.SpotlightId = result.ResponseBody[0].Id;
            illustration.SpotlightTitle = result.ResponseBody[0].Title;
        }

        var entry = result.ResponseBody[0].Entry;
        return new SpotlightDetail(new SpotlightArticle
        {
            Id = entry.Id,
            Title = entry.Title,
            ArticleUrl = entry.ArticleUrl,
            PublishDate = DateTimeOffset.FromUnixTimeSeconds(entry.PublishDate),
            Thumbnail = result.ResponseBody[0].ThumbnailUrl
        }, entry.Intro, illustrations);
    }

    public Task<HttpResponseMessage> PostFollowUserAsync(long id, PrivacyPolicy privacyPolicy)
        => RunWithLoggerAsync(async t => await t
            .FollowUserAsync(new FollowUserRequest(id.ToString(), privacyPolicy.GetDescription()))
            .ConfigureAwait(false));

    public Task<HttpResponseMessage> RemoveFollowUserAsync(long id)
        => RunWithLoggerAsync(async t => await t
            .RemoveFollowUserAsync(new RemoveFollowUserRequest(id.ToString()))
            .ConfigureAwait(false));

    public Task<IEnumerable<TrendingTag>> GetTrendingTagsAsync(TargetFilter targetFilter)
        => RunWithLoggerAsync(async t => (await t
            .GetTrendingTagsAsync(targetFilter.GetDescription())
            .ConfigureAwait(false))
            .TrendTags
            .Select(tag => new TrendingTag(tag.TagStr, tag.TranslatedName, tag.Illust)));

    public Task<IEnumerable<TrendingTag>> GetTrendingTagsForNovelAsync(TargetFilter targetFilter)
        => RunWithLoggerAsync(async t => (await t
            .GetTrendingTagsForNovelAsync(targetFilter.GetDescription())
            .ConfigureAwait(false))
            .TrendTags
            .Select(tag => new TrendingTag(tag.TagStr, tag.TranslatedName, tag.Illust)));

    /// <summary>
    /// Gets the tags that are created by users to classify their bookmarks
    /// </summary>
    /// <example>
    /// <a href="https://www.pixiv.net/en/users/333556/bookmarks/artworks">A user's bookmarks page</a>.
    /// There is a list of tags atop of the illustrations
    /// </example>
    /// <returns>
    /// A <see cref="IReadOnlyDictionary{TKey,TValue}" /> representing the results, where the keys are
    /// tags and values are the privacy of the tags
    /// </returns>
    /// <param name="uid">The ID of the user</param>
    public async Task<IReadOnlyDictionary<CountedTag, PrivacyPolicy>> GetUserSpecifiedBookmarkTagsAsync(long uid)
    {
        EnsureNotCancelled();
        var tags = (await GetMakoHttpClient(MakoApiKind.WebApi)
                .GetStringResultAsync($"/ajax/user/{uid}/illusts/bookmark/tags?lang={Configuration.CultureInfo.TwoLetterISOLanguageName}").ConfigureAwait(false))
            .UnwrapOrThrow()
            .FromJson<UserSpecifiedBookmarkTagResponse>();
        var dic = new Dictionary<CountedTag, PrivacyPolicy>();
        if (tags?.ResponseBody.Public is { } publicTags)
        {
            foreach (var tag in publicTags)
            {
                dic[new CountedTag(new Tag { Name = tag.Name, TranslatedName = null }, tag.Count)] = PrivacyPolicy.Public;
            }
        }

        if (tags?.ResponseBody.Private is { } privateTags)
        {
            foreach (var tag in privateTags)
            {
                dic[new CountedTag(new Tag { Name = tag.Name, TranslatedName = null }, tag.Count)] = PrivacyPolicy.Private;
            }
        }

        return dic;
    }

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
            .AddIllustCommentAsync(new AddNormalIllustCommentRequest(illustId, content)));

    public Task<HttpResponseMessage> AddIllustCommentAsync(long illustId, int stampId)
        => RunWithLoggerAsync(async t => await t
            .AddIllustCommentAsync(new AddStampIllustCommentRequest(illustId, stampId)));

    public Task<HttpResponseMessage> AddIllustCommentAsync(long illustId, long parentCommentId, string content)
        => RunWithLoggerAsync(async t => await t
            .AddIllustCommentAsync(new AddNormalIllustSubCommentRequest(illustId, parentCommentId, content)));

    public Task<HttpResponseMessage> AddIllustCommentAsync(long illustId, long parentCommentId, int stampId)
        => RunWithLoggerAsync(async t => await t
            .AddIllustCommentAsync(new AddStampIllustSubCommentRequest(illustId, parentCommentId, stampId)));

    public Task<HttpResponseMessage> AddNovelCommentAsync(long novelId, string content)
        => RunWithLoggerAsync(async t => await t
            .AddNovelCommentAsync(new AddNormalNovelCommentRequest(novelId, content)));

    public Task<HttpResponseMessage> AddNovelCommentAsync(long novelId, int stampId)
        => RunWithLoggerAsync(async t => await t
            .AddNovelCommentAsync(new AddStampNovelCommentRequest(novelId, stampId)));

    public Task<HttpResponseMessage> AddNovelCommentAsync(long novelId, long parentCommentId, string content)
        => RunWithLoggerAsync(async t => await t
            .AddNovelCommentAsync(new AddNormalNovelSubCommentRequest(novelId, parentCommentId, content)));

    public Task<HttpResponseMessage> AddNovelCommentAsync(long novelId, long parentCommentId, int stampId)
        => RunWithLoggerAsync(async t => await t
            .AddNovelCommentAsync(new AddStampNovelSubCommentRequest(novelId, parentCommentId, stampId)));

    public Task<ReverseSearchResponse> ReverseSearchAsync(Stream imgStream, string apiKey)
        => RunWithLoggerAsync(async () => await MakoServices.GetRequiredService<IReverseSearchApiEndPoint>()
            .GetSauceAsync(new ReverseSearchRequest(apiKey), new StreamPart(imgStream, "img")));
}
