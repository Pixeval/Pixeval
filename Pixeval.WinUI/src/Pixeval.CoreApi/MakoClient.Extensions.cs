using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.EndPoints;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Net.Response;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi
{
    public partial class MakoClient
    {
        /// <summary>
        /// Gets the detail of an illustration from the illust id
        /// </summary>
        /// <param name="id">The illust id</param>
        /// <returns></returns>
        public async Task<Illustration> GetIllustrationFromIdAsync(string id)
        {
            var result = (await Resolve<IAppApiEndPoint>().GetSingle(id)).Illust;
            return result!.ToIllustration(this);
        }

        public async Task<User> GetUserFromIdAsync(string id, TargetFilter targetFilter)
        {
            var result = await Resolve<IAppApiEndPoint>().GetSingleUser(new SingleUserRequest(id, targetFilter.GetDescription()));
            var entity = result.UserEntity;
            return User.GetOrInstantiateAndConfigureUserFromCache(id, this, user =>
            {
                user.Avatar = entity?.ProfileImageUrls?.Medium;
                user.Follows = result.UserProfile?.TotalFollowUsers ?? 0;
                user.Id = entity?.Id.ToString();
                user.Introduction = entity?.Comment;
                user.IsFollowed = entity?.IsFollowed ?? false;
                user.IsPremium = result.UserProfile?.IsPremium ?? false;
                user.Name = entity?.Name;
            });
        }

        /// <summary>
        /// Sets the <see cref="Illustration.IsBookmarked"/> and sends a request to the Pixiv to add it to the bookmark
        /// </summary>
        /// <param name="illustration">The illustration which needs to be bookmarked</param>
        /// <param name="privacyPolicy">Indicates the privacy of the the illustration in the bookmark</param>
        /// <returns>A <see cref="Task"/> represents the operation</returns>
        public Task PostBookmarkAsync(Illustration illustration, PrivacyPolicy privacyPolicy)
        {
            illustration.SetBookmark();
            return PostBookmarkAsync(illustration.Id!, privacyPolicy);
        }

        /// <summary>
        /// Unsets the <see cref="Illustration.IsBookmarked"/> and sends a request to the Pixiv to remove it from the bookmark
        /// </summary>
        /// <param name="illustration">The illustration which needs to be removed from the bookmark</param>
        /// <returns>A <see cref="Task"/> represents the operation</returns>
        public Task RemoveBookmarkAsync(Illustration illustration)
        {
            illustration.UnsetBookmark();
            return RemoveBookmarkAsync(illustration.Id!);
        }

        /// <summary>
        /// Sends a request to the Pixiv to add it to the bookmark
        /// </summary>
        /// <param name="id">The ID of the illustration which needs to be bookmarked</param>
        /// <param name="privacyPolicy">Indicates the privacy of the the illustration in the bookmark</param>
        /// <returns>A <see cref="Task"/> represents the operation</returns>
        public Task PostBookmarkAsync(string id, PrivacyPolicy privacyPolicy)
        {
            return Resolve<IAppApiEndPoint>().AddBookmark(new AddBookmarkRequest(privacyPolicy.GetDescription(), id));
        }

        /// <summary>
        /// Sends a request to the Pixiv to remove it from the bookmark
        /// </summary>
        /// <param name="id">The ID of the illustration which needs to be removed from the bookmark</param>
        /// <returns>A <see cref="Task"/> represents the operation</returns>
        public Task RemoveBookmarkAsync(string id)
        {
            return Resolve<IAppApiEndPoint>().RemoveBookmark(new RemoveBookmarkRequest(id));
        }

        /// <summary>
        /// Gets the details of a spotlight from its ID which contains the article information, introduction and illustrations
        /// </summary>
        /// <param name="spotlightId">The ID of the spotlight</param>
        /// <returns>A <see cref="Task{TResult}"/> contains the result of the operation</returns>
        public async Task<SpotlightDetail?> GetSpotlightDetailAsync(string spotlightId)
        {
            var result = (await GetMakoHttpClient(MakoApiKind.WebApi).GetStringResultAsync($"/ajax/showcase/article?article_id={spotlightId}",
                message => MakoNetworkException.FromHttpResponseMessage(message, Configuration.Bypass))).GetOrThrow().FromJson<PixivSpotlightDetailResponse>();
            if (result?.ResponseBody is null) return null;
            var illustrations = await (result.ResponseBody.First().Illusts?.SelectNotNull(illust => Task.Run(() => GetIllustrationFromIdAsync(illust.IllustId.ToString()))).WhenAll() ?? Task.FromResult(Array.Empty<Illustration>()));
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

        public Task PostFollowUserAsync(User user, PrivacyPolicy privacyPolicy)
        {
            return user.IsFollowed ? Task.CompletedTask : PostFollowUserAsync(user.Id!, privacyPolicy);
        }

        public Task PostFollowUserAsync(string id, PrivacyPolicy privacyPolicy)
        {
            return Resolve<IAppApiEndPoint>().FollowUser(new FollowUserRequest(id, privacyPolicy.GetDescription()));
        }

        public Task RemoveFollowUserAsync(User user)
        {
            return user.IsFollowed ? RemoveFollowUserAsync(user.Id!) : Task.CompletedTask;
        }

        public Task RemoveFollowUserAsync(string id)
        {
            return Resolve<IAppApiEndPoint>().RemoveFollowUser(new RemoveFollowUserRequest(id));
        }

        public async Task<IEnumerable<TrendingTag>> GetTrendingTagsAsync(TargetFilter targetFilter)
        {
            return ((await Resolve<IAppApiEndPoint>().GetTrendingTags(targetFilter.GetDescription())).TrendTags ?? Enumerable.Empty<TrendingTagResponse.TrendTag>()).Select(t => new TrendingTag
            {
                Tag = t.TagStr,
                Translation = t.TranslatedName,
                Illustration = t.Illust?.ToIllustration(this)
            });
        }

        /// <summary>
        /// Get the tags that are created by users to classify their bookmarks
        /// </summary>
        /// <example>
        /// <a href="https://www.pixiv.net/en/users/333556/bookmarks/artworks">A user's bookmarks page</a>. 
        /// There is a list of tags atop of the illustrations
        /// </example>
        /// <returns>
        /// An <see cref="IReadOnlyDictionary{TKey,TValue}"/> representing the results, where the keys are
        /// tags and values are the privacy of the key
        /// </returns>
        /// <param name="uid">The user id that you want to </param>
        public async Task<IReadOnlyDictionary<CountedTag, PrivacyPolicy>> GetUserSpecifiedBookmarkTags(string uid)
        {
            var tags = (await GetMakoHttpClient(MakoApiKind.WebApi).GetStringResultAsync($"/ajax/user/{uid}/illusts/bookmark/tags?lang={Configuration.CultureInfo.TwoLetterISOLanguageName}"))
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
    }
}