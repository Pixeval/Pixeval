#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/MakoClient.Engines.cs
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
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Engine.Implements;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Global.Exception;
using Pixeval.CoreApi.Model;
using Pixeval.Utilities;

namespace Pixeval.CoreApi;

public partial class MakoClient
{
    // --------------------------------------------------
    // This part contains all APIs that depend on the
    // IFetchEngine, however, the uniqueness of the inner
    // elements is not guaranteed, call Distinct() if you
    // are care about the uniqueness of the results
    // --------------------------------------------------

    /// <summary>
    ///     Request bookmarked illustrations for a user.
    /// </summary>
    /// <param name="uid">User id</param>
    /// <param name="privacyPolicy">The <see cref="PrivacyPolicy" /> options targeting private or public</param>
    /// <param name="targetFilter">The <see cref="TargetFilter" /> options targeting android or ios</param>
    /// <returns>
    ///     The <see cref="BookmarkEngine" />> iterator containing bookmarked illustrations for the user.
    /// </returns>
    /// <exception cref="IllegalPrivatePolicyException">Requesting other user's private bookmarks will throw this exception.</exception>
    public IFetchEngine<Illustration> Bookmarks(string uid, PrivacyPolicy privacyPolicy, TargetFilter targetFilter = TargetFilter.ForAndroid)
    {
        EnsureNotCancelled();
        if (!CheckPrivacyPolicy(uid, privacyPolicy))
        {
            throw new IllegalPrivatePolicyException(uid);
        }

        return new BookmarkEngine(this, uid, privacyPolicy, targetFilter, new EngineHandle(CancelInstance)).Apply(RegisterInstance);
    }

    /// <summary>
    ///     Search in Pixiv.
    /// </summary>
    /// <param name="tag">Texts for searching</param>
    /// <param name="start">Start page</param>
    /// <param name="pages">Number of pages</param>
    /// <param name="matchOption">
    ///     The <see cref="SearchTagMatchOption.TitleAndCaption" /> option for the method of search
    ///     matching
    /// </param>
    /// <param name="sortOption">The <see cref="IllustrationSortOption" /> option for sorting method</param>
    /// <param name="searchDuration">The <see cref="SearchDuration" /> option for the duration of this search</param>
    /// <param name="targetFilter">The <see cref="TargetFilter" /> option targeting android or ios</param>
    /// <param name="startDate">The starting date filtering the search results</param>
    /// <param name="endDate">The ending date filtering the searching results</param>
    /// <returns>
    ///     The <see cref="SearchEngine" /> iterator containing the searching results.
    /// </returns>
    public IFetchEngine<Illustration> Search(
        string tag,
        int start = 0,
        int pages = 100,
        SearchTagMatchOption matchOption = SearchTagMatchOption.TitleAndCaption,
        IllustrationSortOption? sortOption = null,
        SearchDuration searchDuration = SearchDuration.Undecided,
        TargetFilter? targetFilter = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null)
    {
        EnsureNotCancelled();
        if (sortOption == IllustrationSortOption.PopularityDescending && !Session.IsPremium)
        {
            sortOption = IllustrationSortOption.DoNotSort;
        }

        return new SearchEngine(this, new EngineHandle(CancelInstance), matchOption, tag, start, pages, sortOption, searchDuration, startDate, endDate, targetFilter);
    }

    /// <summary>
    ///     Request ranking illustrations in Pixiv.
    /// </summary>
    /// <param name="rankOption">The option of which the <see cref="RankOption" /> of rankings</param>
    /// <param name="dateTime">The date of rankings</param>
    /// <param name="targetFilter">The <see cref="TargetFilter" /> option targeting android or ios</param>
    /// <returns>
    ///     The <see cref="RankingEngine" /> containing rankings.
    /// </returns>
    /// <exception cref="RankingDateOutOfRangeException">
    ///     Throw this exception if the date is not valid.
    /// </exception>
    public IFetchEngine<Illustration> Ranking(RankOption rankOption, DateTime dateTime, TargetFilter targetFilter = TargetFilter.ForAndroid)
    {
        EnsureNotCancelled();
        if (DateTime.Today - dateTime.Date < TimeSpan.FromDays(2))
        {
            throw new RankingDateOutOfRangeException();
        }

        return new RankingEngine(this, rankOption, dateTime, targetFilter, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request recommended illustrations in Pixiv.
    /// </summary>
    /// <param name="recommendContentType">The <see cref="RecommendationContentType" /> option for illust or manga</param>
    /// <param name="targetFilter">The <see cref="TargetFilter" /> option targeting android or ios</param>
    /// <param name="maxBookmarkIdForRecommend">Max bookmark id for recommendation</param>
    /// <param name="minBookmarkIdForRecentIllust">Min bookmark id for recent illust</param>
    /// <returns>
    ///     The <see cref="RecommendationEngine" /> containing recommended illustrations.
    /// </returns>
    public IFetchEngine<Illustration> Recommendations(
        RecommendationContentType recommendContentType = RecommendationContentType.Illust,
        TargetFilter targetFilter = TargetFilter.ForAndroid,
        uint? maxBookmarkIdForRecommend = null,
        uint? minBookmarkIdForRecentIllust = null)
    {
        EnsureNotCancelled();
        return new RecommendationEngine(this, recommendContentType, targetFilter, maxBookmarkIdForRecommend, minBookmarkIdForRecentIllust, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request recommended illustrators.
    /// </summary>
    /// <param name="targetFilter">The <see cref="TargetFilter" /> option targeting android or ios</param>
    /// <returns>
    ///     The <see cref="RecommendIllustratorEngine" /> containing recommended illustrators.
    /// </returns>
    public IFetchEngine<User> RecommendIllustrators(TargetFilter targetFilter = TargetFilter.ForAndroid)
    {
        EnsureNotCancelled();
        return new RecommendIllustratorEngine(this, targetFilter, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request the spotlights in Pixiv.
    /// </summary>
    /// <returns>
    ///     The <see cref="SpotlightArticleEngine" /> containing the spotlight articles.
    /// </returns>
    public IFetchEngine<SpotlightArticle> Spotlights()
    {
        EnsureNotCancelled();
        return new SpotlightArticleEngine(this, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request feeds (the recent activity of following users)
    /// </summary>
    /// <returns>
    ///     The <see cref="FeedEngine" /> containing the feeds.
    /// </returns>
    public IFetchEngine<Feed> Feeds()
    {
        EnsureNotCancelled();
        return new FeedEngine(this, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request posts of a user.
    /// </summary>
    /// <param name="uid">User id.</param>
    /// <returns>
    ///     The <see cref="PostedIllustrationEngine" /> containing posts of that user.
    /// </returns>
    public IFetchEngine<Illustration> Posts(string uid)
    {
        EnsureNotCancelled();
        return new PostedIllustrationEngine(this, uid, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request following users of a user.
    /// </summary>
    /// <param name="uid">User id</param>
    /// <param name="privacyPolicy">The <see cref="PrivacyPolicy" /> options targeting private or public</param>
    /// <returns>
    ///     The <see cref="FollowingEngine" /> containing following users.
    /// </returns>
    /// <exception cref="IllegalPrivatePolicyException"></exception>
    public IFetchEngine<User?> Following(string uid, PrivacyPolicy privacyPolicy)
    {
        EnsureNotCancelled();
        if (!CheckPrivacyPolicy(uid, privacyPolicy))
        {
            throw new IllegalPrivatePolicyException(uid);
        }

        return new FollowingEngine(this, privacyPolicy, uid, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Search user in Pixiv.
    /// </summary>
    /// <param name="keyword">The text in searching</param>
    /// <param name="userSortOption">The <see cref="UserSortOption" /> enum as date ascending or descending.</param>
    /// <param name="targetFilter">The <see cref="TargetFilter" /> option targeting android or ios</param>
    /// <returns>
    ///     The <see cref="UserSearchEngine" /> containing the search results for users.
    /// </returns>
    public IFetchEngine<User> SearchUser(
        string keyword,
        UserSortOption userSortOption = UserSortOption.DateDescending,
        TargetFilter targetFilter = TargetFilter.ForAndroid)
    {
        EnsureNotCancelled();
        return new UserSearchEngine(this, targetFilter, userSortOption, keyword, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request recent posts of following users.
    /// </summary>
    /// <param name="privacyPolicy">The <see cref="PrivacyPolicy" /> options targeting private or public</param>
    /// <returns>
    ///     The <see cref="RecentPostedIllustrationEngine" /> containing the recent posts.
    /// </returns>
    public IFetchEngine<Illustration> RecentPosts(PrivacyPolicy privacyPolicy)
    {
        EnsureNotCancelled();
        return new RecentPostedIllustrationEngine(this, privacyPolicy, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     This function is intended to be cooperated with <see cref="GetUserSpecifiedBookmarkTagsAsync" />, because
    ///     it requires an untranslated tag, for example, "未分類" is the untranslated name for "uncategorized",
    ///     and the API only recognizes the former one, while the latter one is usually works as the display
    ///     name
    /// </summary>
    /// <param name="uid">User id</param>
    /// <param name="tagWithOriginalName">The untranslated name of the tag</param>
    /// <returns>
    ///     The <see cref="TaggedBookmarksIdEngine" /> containing the illustrations ID for the bookmark tag.
    /// </returns>
    public IFetchEngine<string> UserTaggedBookmarksId(string uid, string tagWithOriginalName)
    {
        EnsureNotCancelled();
        return new TaggedBookmarksIdEngine(this, new EngineHandle(CancelInstance), uid, tagWithOriginalName);
    }

    /// <summary>
    ///     Similar to <see cref="UserTaggedBookmarksId" /> but get the illustrations.
    /// </summary>
    /// <param name="uid">User id</param>
    /// <param name="tagWithOriginalName">The untranslated name of the tag</param>
    /// <returns>
    ///     The <see cref="TaggedBookmarksIdEngine" /> containing the illustrations for the bookmark tag.
    /// </returns>
    public IFetchEngine<Illustration> UserTaggedBookmarks(string uid, string tagWithOriginalName)
    {
        EnsureNotCancelled();
        return new FetchEngineSelector<string, Illustration>(new TaggedBookmarksIdEngine(this, new EngineHandle(CancelInstance), uid, tagWithOriginalName), GetIllustrationFromIdAsync);
    }

    /// <summary>
    ///     Request manga posts of that user.
    /// </summary>
    /// <param name="uid">User id</param>
    /// <param name="targetFilter">The <see cref="TargetFilter" /> option targeting android or ios</param>
    /// <returns>
    ///     The <see cref="PostedMangaEngine" /> containing the manga posts of the user.
    /// </returns>
    public IFetchEngine<Illustration> MangaPosts(string uid, TargetFilter targetFilter)
    {
        EnsureNotCancelled();
        return new PostedMangaEngine(this, uid, targetFilter, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request novel posts of that user.
    /// </summary>
    /// <param name="uid">User id</param>
    /// <param name="targetFilter">The <see cref="TargetFilter" /> option targeting android or ios</param>
    /// <returns>
    ///     The <see cref="PostedNovelEngine" /> containing the novel posts of that user.
    /// </returns>
    public IFetchEngine<Novel> NovelPosts(string uid, TargetFilter targetFilter)
    {
        EnsureNotCancelled();
        return new PostedNovelEngine(this, uid, targetFilter, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request bookmarked novels.
    /// </summary>
    /// <param name="uid">User id</param>
    /// <param name="privacyPolicy">The <see cref="PrivacyPolicy" /> options targeting private or public</param>
    /// <param name="targetFilter">The <see cref="TargetFilter" /> option targeting android or ios</param>
    /// <returns>
    ///     The <see cref="NovelBookmarkEngine" /> containing the bookmarked novels.
    /// </returns>
    public IFetchEngine<Novel> NovelBookmarks(string uid, PrivacyPolicy privacyPolicy, TargetFilter targetFilter)
    {
        EnsureNotCancelled();
        CheckPrivacyPolicy(uid, privacyPolicy);
        return new NovelBookmarkEngine(this, uid, privacyPolicy, targetFilter, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request comments of an illustration.
    /// </summary>
    /// <param name="illustId">Illustration id</param>
    /// <returns>
    ///     The <see cref="IllustrationCommentsEngine" /> containing comments of the illustration.
    /// </returns>
    public IFetchEngine<Comment?> IllustrationComments(string illustId)
    {
        EnsureNotCancelled();
        return new IllustrationCommentsEngine(illustId, this, new EngineHandle(CancelInstance));
    }

    /// <summary>
    ///     Request replies of a comment.
    /// </summary>
    /// <param name="commentId">Comment id</param>
    /// <returns>
    ///     The <see cref="IllustrationCommentRepliesEngine" /> containing replies of the comment.
    /// </returns>
    public IFetchEngine<Comment> IllustrationCommentReplies(string commentId)
    {
        EnsureNotCancelled();
        return new IllustrationCommentRepliesEngine(commentId, this, new EngineHandle(CancelInstance));
    }
}