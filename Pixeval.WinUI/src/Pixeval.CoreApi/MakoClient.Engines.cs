using System;
using Pixeval.CoreApi.Engines;
using Pixeval.CoreApi.Engines.Implements;
using Pixeval.CoreApi.Model;
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
        private void EnsureNotCancelled()
        {
            if (CancellationTokenSource.IsCancellationRequested)
            {
                throw new OperationCanceledException("This client has been cancelled");
            }
        }
        
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
                throw new IllegalSortOptionException();
            }

            return new SearchEngine(this, new EngineHandle(CancelInstance), matchOption, tag, start, pages, sortOption, searchDuration, startDate, endDate, targetFilter);
        }

        public IFetchEngine<Illustration> Ranking(RankOption rankOption, DateTime dateTime, TargetFilter targetFilter = TargetFilter.ForAndroid)
        {
            EnsureNotCancelled();
            if (DateTime.Today - dateTime.Date > TimeSpan.FromDays(2))
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
            return new UserFeedEngine(this, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<Illustration> Uploads(string uid)
        {
            EnsureNotCancelled();
            return new UserUploadEngine(this, uid, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<User> Following(string uid, PrivacyPolicy privacyPolicy)
        {
            EnsureNotCancelled();
            if (!CheckPrivacyPolicy(uid, privacyPolicy))
            {
                throw new IllegalPrivatePolicyException(uid);
            }

            return new UserFollowingEngine(this, privacyPolicy, uid, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<User> SearchUser(
            string keyword,
            UserSortOption userSortOption = UserSortOption.DateDescending,
            TargetFilter targetFilter = TargetFilter.ForAndroid)
        {
            EnsureNotCancelled();
            return new UserSearchEngine(this, targetFilter, userSortOption, keyword, new EngineHandle(CancelInstance));
        }

        public IFetchEngine<Illustration> Updates(PrivacyPolicy privacyPolicy)
        {
            EnsureNotCancelled();
            return new UserUpdateEngine(this, privacyPolicy, new EngineHandle(CancelInstance));
        }

        /// <summary>
        /// This function is intended to be cooperated with <see cref="GetUserSpecifiedBookmarkTags"/>, because
        /// it requires an untranslated tag, for example, "未分類" is the untranslated name for "uncategorized",
        /// and the API only recognizes the former one, while the latter one is usually works as the display
        /// name
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="tagWithOriginalName"></param>
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
    }
}