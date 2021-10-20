#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/MakoClient.Extensions.cs
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

using System;
using System.Collections.Generic;
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

namespace Pixeval.CoreApi
{
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

        public async Task<User.Info> GetUserFromIdAsync(string id, TargetFilter targetFilter)
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
                publicTags.ForEach(tag => dic[new CountedTag(new Tag {Name = tag.Name}, tag.Count)] = PrivacyPolicy.Public);
            }

            if (tags?.ResponseBody?.Private is { } privateTags)
            {
                privateTags.ForEach(tag => dic[new CountedTag(new Tag {Name = tag.Name}, tag.Count)] = PrivacyPolicy.Private);
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
    }
}