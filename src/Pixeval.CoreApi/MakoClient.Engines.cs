#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/MakoClient.Engines.cs
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
using Pixeval.CoreApi.Engine;
using Pixeval.CoreApi.Engine.Implements;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Global.Exception;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net.Response;
using Pixeval.CoreApi.Util;

namespace Pixeval.CoreApi
{
    public partial class MakoClient
    {
        // --------------------------------------------------
        // This part contains all APIs that depend on the
        // IFetchEngine, however, the uniqueness of the inner
        // elements is not guaranteed, call Distinct() if you
        // are care about the uniqueness of the results
        // --------------------------------------------------

        public IFetchEngine<Illustration> Bookmarks(string uid, PrivacyPolicy privacyPolicy, TargetFilter targetFilter = TargetFilter.ForAndroid)
        {
            EnsureNotCancelled();
            if (!CheckPrivacyPolicy(uid, privacyPolicy))
            {
                throw new IllegalPrivatePolicyException(uid);
            }

            return new BookmarkEngine(this, uid, privacyPolicy, targetFilter, new EngineHandle(CancelInstance)).Apply(RegisterInstance);
        }

        public IFetchEngine<Illustration> Search(
            string tag,
            int start = 0,
            int pages = 100,
            SearchTagMatchOption matchOption = SearchTagMatchOption.TitleAndCaption,
            IllustrationSortOption? sortOption = null,
            SearchDuration? searchDuration = null,
            TargetFilter? targetFilter = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            EnsureNotCancelled();
            if (sortOption == IllustrationSortOption.PopularityDescending && !Session.IsPremium)
            {
                sortOption = IllustrationSortOption.DoNotSort;
            }

            return new SearchEngine(this, new EngineHandle(CancelInstance), matchOption, tag, start, pages, sortOption, searchDuration, startDate, endDate, targetFilter);
        }

        public IFetchEngine<Illustration> Ranking(RankOption rankOption, DateTime dateTime, TargetFilter targetFilter = TargetFilter.ForAndroid)
        {
            EnsureNotCancelled();
            if (DateTime.Today - dateTime.Date < TimeSpan.FromDays(2))
            {
                throw new RankingDateOutOfRangeException();
            }

            return new RankingEngine(this, rankOption, dateTime, targetFilter, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<Illustration> Recommends(
            RecommendContentType recommendContentType = RecommendContentType.Illust,
            TargetFilter targetFilter = TargetFilter.ForAndroid,
            uint? maxBookmarkIdForRecommend = null,
            uint? maxBookmarkIdForRecentIllust = null)
        {
            EnsureNotCancelled();
            return new RecommendEngine(this, recommendContentType, targetFilter, maxBookmarkIdForRecommend, maxBookmarkIdForRecentIllust, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<User> RecommendIllustrators(TargetFilter targetFilter = TargetFilter.ForAndroid)
        {
            EnsureNotCancelled();
            return new RecommendIllustratorEngine(this, targetFilter, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<SpotlightArticle> Spotlights()
        {
            EnsureNotCancelled();
            return new SpotlightArticleEngine(this, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<Feed> Feeds()
        {
            EnsureNotCancelled();
            return new FeedEngine(this, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<Illustration> Posts(string uid)
        {
            EnsureNotCancelled();
            return new PostedIllustrationEngine(this, uid, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<User> Following(string uid, PrivacyPolicy privacyPolicy)
        {
            EnsureNotCancelled();
            if (!CheckPrivacyPolicy(uid, privacyPolicy))
            {
                throw new IllegalPrivatePolicyException(uid);
            }

            return new FollowingEngine(this, privacyPolicy, uid, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<User> SearchUser(
            string keyword,
            UserSortOption userSortOption = UserSortOption.DateDescending,
            TargetFilter targetFilter = TargetFilter.ForAndroid)
        {
            EnsureNotCancelled();
            return new UserSearchEngine(this, targetFilter, userSortOption, keyword, new EngineHandle(CancelInstance));
        }

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
        /// <param name="uid"></param>
        /// <param name="tagWithOriginalName">The untranslated name of the tag</param>
        /// <returns></returns>
        public IFetchEngine<string> UserTaggedBookmarksId(string uid, string tagWithOriginalName)
        {
            EnsureNotCancelled();
            return new TaggedBookmarksIdEngine(this, new EngineHandle(CancelInstance), uid, tagWithOriginalName);
        }

        public IFetchEngine<Illustration> UserTaggedBookmarks(string uid, string tagWithOriginalName)
        {
            EnsureNotCancelled();
            return new FetchEngineSelector<string, Illustration>(new TaggedBookmarksIdEngine(this, new EngineHandle(CancelInstance), uid, tagWithOriginalName), GetIllustrationFromIdAsync);
        }

        public IFetchEngine<Illustration> MangaPosts(string uid, TargetFilter targetFilter)
        {
            EnsureNotCancelled();
            return new PostedMangaEngine(this, uid, targetFilter, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<Novel> NovelPosts(string uid, TargetFilter targetFilter)
        {
            EnsureNotCancelled();
            return new PostedNovelEngine(this, uid, targetFilter, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<Novel> NovelBookmarks(string uid, PrivacyPolicy privacyPolicy, TargetFilter targetFilter)
        {
            EnsureNotCancelled();
            CheckPrivacyPolicy(uid, privacyPolicy);
            return new NovelBookmarkEngine(this, uid, privacyPolicy, targetFilter, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<IllustrationCommentsResponse.Comment> IllustrationComments(string illustId)
        {
            EnsureNotCancelled();
            return new IllustrationCommentsEngine(illustId, this, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<IllustrationCommentsResponse.Comment> IllustrationCommentReplies(string commentId)
        {
            EnsureNotCancelled();
            return new IllustrationCommentRepliesEngine(commentId, this, new EngineHandle(CancelInstance));
        }
    }
}