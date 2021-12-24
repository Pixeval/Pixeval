﻿#region Copyright (c) Pixeval/Pixeval.CoreApi

// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/MakoClient.Extensions.cs
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    /// <summary>
    ///     Gets the detail of an illustration from the illust id
    /// </summary>
    /// <param name="id">The illust id</param>
    /// <returns></returns>
    public async Task<Illustration> GetIllustrationFromIdAsync(string id)
    {
        EnsureNotCancelled();
        return (await Resolve<IAppApiEndPoint>().GetSingleAsync(id).ConfigureAwait(false)).Illust!;
    }

    public async Task<IEnumerable<Tag>> GetAutoCompletionForKeyword(string word)
    {
        return (await Resolve<IAppApiEndPoint>().GetAutoCompletionAsync(new AutoCompletionRequest(word))).Tags!;
    }

    public async Task<UserInfo> GetUserFromIdAsync(string id, TargetFilter targetFilter)
    {
        EnsureNotCancelled();
        var result = await Resolve<IAppApiEndPoint>().GetSingleUserAsync(new SingleUserRequest(id, targetFilter.GetDescription())).ConfigureAwait(false);
        return result.UserEntity!;
    }

    /// <summary>
    ///     Sends a request to the Pixiv to add it to the bookmark
    /// </summary>
    /// <param name="id">The ID of the illustration which needs to be bookmarked</param>
    /// <param name="privacyPolicy">Indicates the privacy of the the illustration in the bookmark</param>
    /// <returns>A <see cref="Task" /> represents the operation</returns>
    public Task PostBookmarkAsync(string id, PrivacyPolicy privacyPolicy)
    {
        EnsureNotCancelled();
        return Resolve<IAppApiEndPoint>().AddBookmarkAsync(new AddBookmarkRequest(privacyPolicy.GetDescription(), id));
    }

    /// <summary>
    ///     Sends a request to the Pixiv to remove it from the bookmark
    /// </summary>
    /// <param name="id">The ID of the illustration which needs to be removed from the bookmark</param>
    /// <returns>A <see cref="Task" /> represents the operation</returns>
    public Task RemoveBookmarkAsync(string id)
    {
        EnsureNotCancelled();
        return Resolve<IAppApiEndPoint>().RemoveBookmarkAsync(new RemoveBookmarkRequest(id));
    }

    /// <summary>
    ///     Gets the details of a spotlight from its ID which contains the article information, introduction, and illustrations
    /// </summary>
    /// <param name="spotlightId">The ID of the spotlight</param>
    /// <returns>A <see cref="Task{TResult}" /> contains the result of the operation</returns>
    public async Task<SpotlightDetail?> GetSpotlightDetailAsync(string spotlightId)
    {
        EnsureNotCancelled();
        var result = (await GetMakoHttpClient(MakoApiKind.WebApi)
                .GetStringResultAsync($"/ajax/showcase/article?article_id={spotlightId}",
                    message => MakoNetworkException.FromHttpResponseMessageAsync(message, Configuration.Bypass))
                .ConfigureAwait(false))
            .GetOrThrow().FromJson<PixivSpotlightDetailResponse>();
        if (result?.ResponseBody is null)
        {
            return null;
        }

        var illustrations = await (result.ResponseBody.First().Illusts?.SelectNotNull(illust => Task.Run(() => GetIllustrationFromIdAsync(illust.IllustId.ToString()))).WhenAll()
                                   ?? Task.FromResult(Array.Empty<Illustration>())).ConfigureAwait(false);
        foreach (var illustration in illustrations)
        {
            illustration.FromSpotlight = true;
            illustration.SpotlightId = result.ResponseBody.First().Id;
            illustration.SpotlightTitle = result.ResponseBody.First().Title;
        }

        var entry = result.ResponseBody.First().Entry;
        return new SpotlightDetail(new SpotlightArticle
        {
            Id = long.Parse(entry?.Id ?? "0"),
            Title = entry?.Title,
            ArticleUrl = entry?.ArticleUrl,
            PublishDate = DateTimeOffset.FromUnixTimeSeconds(entry?.PublishDate ?? 0),
            Thumbnail = result.ResponseBody.First().ThumbnailUrl
        }, entry?.Intro ?? string.Empty, illustrations);
    }

    public Task PostFollowUserAsync(string id, PrivacyPolicy privacyPolicy)
    {
        EnsureNotCancelled();
        return Resolve<IAppApiEndPoint>().FollowUserAsync(new FollowUserRequest(id, privacyPolicy.GetDescription()));
    }

    public Task RemoveFollowUserAsync(string id)
    {
        EnsureNotCancelled();
        return Resolve<IAppApiEndPoint>().RemoveFollowUserAsync(new RemoveFollowUserRequest(id));
    }

    public async Task<IEnumerable<TrendingTag>> GetTrendingTagsAsync(TargetFilter targetFilter)
    {
        EnsureNotCancelled();
        return ((await Resolve<IAppApiEndPoint>().GetTrendingTagsAsync(targetFilter.GetDescription()).ConfigureAwait(false)).TrendTags ?? Enumerable.Empty<TrendingTagResponse.TrendTag>()).Select(t => new TrendingTag
        {
            Tag = t.TagStr,
            Translation = t.TranslatedName,
            Illustration = t.Illust
        });
    }

    /// <summary>
    ///     Gets the tags that are created by users to classify their bookmarks
    /// </summary>
    /// <example>
    ///     <a href="https://www.pixiv.net/en/users/333556/bookmarks/artworks">A user's bookmarks page</a>.
    ///     There is a list of tags atop of the illustrations
    /// </example>
    /// <returns>
    ///     An <see cref="IReadOnlyDictionary{TKey,TValue}" /> representing the results, where the keys are
    ///     tags and values are the privacy of the tags
    /// </returns>
    /// <param name="uid">The ID of the user</param>
    public async Task<IReadOnlyDictionary<CountedTag, PrivacyPolicy>> GetUserSpecifiedBookmarkTagsAsync(string uid)
    {
        EnsureNotCancelled();
        var tags = (await GetMakoHttpClient(MakoApiKind.WebApi).GetStringResultAsync($"/ajax/user/{uid}/illusts/bookmark/tags?lang={Configuration.CultureInfo.TwoLetterISOLanguageName}").ConfigureAwait(false))
            .GetOrThrow()
            .FromJson<UserSpecifiedBookmarkTagResponse>();
        var dic = new Dictionary<CountedTag, PrivacyPolicy>();
        if (tags?.ResponseBody?.Public is { } publicTags)
        {
            publicTags.ForEach(tag => dic[new CountedTag(new Tag { Name = tag.Name }, tag.Count)] = PrivacyPolicy.Public);
        }

        if (tags?.ResponseBody?.Private is { } privateTags)
        {
            privateTags.ForEach(tag => dic[new CountedTag(new Tag { Name = tag.Name }, tag.Count)] = PrivacyPolicy.Private);
        }

        return dic;
    }

    public Task<UgoiraMetadataResponse> GetUgoiraMetadataAsync(string id)
    {
        return Resolve<IAppApiEndPoint>().GetUgoiraMetadataAsync(id);
    }

    public Task DeleteCommentAsync(string commentId)
    {
        return Resolve<IAppApiEndPoint>().DeleteCommentAsync(new DeleteCommentRequest(commentId));
    }

    public Task<ReverseSearchResponse> ReverseSearchAsync(Stream imgStream, string apiKey)
    {
        return Resolve<IReverseSearchApiEndPoint>().GetSauceAsync(new ReverseSearchRequest(apiKey), new StreamPart(imgStream, "img"));
    }
}