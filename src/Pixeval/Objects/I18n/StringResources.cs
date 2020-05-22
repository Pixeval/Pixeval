#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using PropertyChanged;

namespace Pixeval.Objects.I18n
{
    [AddINotifyPropertyChangedInterface]
    public static partial class AkaI18N
    {
        public static string EmptyEmailOrPasswordIsNotAllowed => GetResource(nameof(EmptyEmailOrPasswordIsNotAllowed));

        public static string IdDoNotExists => GetResource(nameof(IdDoNotExists));

        public static string CannotFindUser => GetResource(nameof(CannotFindUser));

        public static string InputIsEmpty => GetResource(nameof(InputIsEmpty));

        public static string QueryNotResponding => GetResource(nameof(QueryNotResponding));

        public static string IdIllegal => GetResource(nameof(IdIllegal));

        public static string UserIdIllegal => GetResource(nameof(UserIdIllegal));

        public static string AppApiAuthenticateTimeout => GetResource(nameof(AppApiAuthenticateTimeout));

        public static string WebApiAuthenticateTimeout => GetResource(nameof(WebApiAuthenticateTimeout));

        public static string MultiplePixevalInstanceDetected => GetResource(nameof(MultiplePixevalInstanceDetected));

        public static string MultiplePixevalInstanceDetectedTitle =>
            GetResource(nameof(MultiplePixevalInstanceDetectedTitle));

        public static string CppRedistributableRequired => GetResource(nameof(CppRedistributableRequired));

        public static string CppRedistributableRequiredTitle => GetResource(nameof(CppRedistributableRequiredTitle));

        public static string CertificateInstallationIsRequired =>
            GetResource(nameof(CertificateInstallationIsRequired));

        public static string CertificateInstallationIsRequiredTitle =>
            GetResource(nameof(CertificateInstallationIsRequiredTitle));

        public static string TrendsAddIllust => GetResource(nameof(TrendsAddIllust));

        public static string TrendsAddBookmark => GetResource(nameof(TrendsAddBookmark));

        public static string TrendsAddFavorite => GetResource(nameof(TrendsAddFavorite));

        public static string SearchingTrends => GetResource(nameof(SearchingTrends));

        public static string SearchingUserUpdates => GetResource(nameof(SearchingUserUpdates));

        public static string SearchingGallery => GetResource(nameof(SearchingGallery));

        public static string SearchingRecommend => GetResource(nameof(SearchingRecommend));

        public static string SearchingFollower => GetResource(nameof(SearchingFollower));

        public static string SearchingSpotlight => GetResource(nameof(SearchingSpotlight));

        public static string QueuedDownload => GetResource(nameof(QueuedDownload));

        public static string QueuedAllToDownload => GetResource(nameof(QueuedAllToDownload));

        public static string ShareLinkCopiedToClipboard => GetResource(nameof(ShareLinkCopiedToClipboard));

        public static string PathNotExist => GetResource(nameof(PathNotExist));

        public static string SauceNaoFileCountLimit => GetResource(nameof(SauceNaoFileCountLimit));

        public static string CannotFindResult => GetResource(nameof(CannotFindResult));

        public static string PleaseSelectFile => GetResource(nameof(PleaseSelectFile));

        public static string PleaseSelectLocation => GetResource(nameof(PleaseSelectLocation));

        public static string CannotRetrieveContentLengthHeader =>
            GetResource(nameof(CannotRetrieveContentLengthHeader));

        public static string ToggleR18OnSuccess => GetResource(nameof(ToggleR18OnSuccess));

        public static string ToggleR18OnFailed => GetResource(nameof(ToggleR18OnFailed));

        public static string TryingToToggleR18Switch => GetResource(nameof(TryingToToggleR18Switch));

        public static string RankOptionDay => GetResource(nameof(RankOptionDay));

        public static string RankOptionWeek => GetResource(nameof(RankOptionWeek));

        public static string RankOptionMonth => GetResource(nameof(RankOptionMonth));

        public static string RankOptionDayMale => GetResource(nameof(RankOptionDayMale));

        public static string RankOptionDayFemale => GetResource(nameof(RankOptionDayFemale));

        public static string RankOptionDayManga => GetResource(nameof(RankOptionDayManga));

        public static string RankOptionWeekManga => GetResource(nameof(RankOptionWeekManga));

        public static string RankOptionWeekOriginal => GetResource(nameof(RankOptionWeekOriginal));

        public static string RankOptionWeekRookie => GetResource(nameof(RankOptionWeekRookie));

        public static string RankOptionDayR18 => GetResource(nameof(RankOptionDayR18));

        public static string RankOptionDayMaleR18 => GetResource(nameof(RankOptionDayMaleR18));

        public static string RankOptionDayFemaleR18 => GetResource(nameof(RankOptionDayFemaleR18));

        public static string RankOptionWeekR18 => GetResource(nameof(RankOptionWeekR18));

        public static string RankOptionWeekR18G => GetResource(nameof(RankOptionWeekR18G));

        public static string RankDateCannotBeNull => GetResource(nameof(RankDateCannotBeNull));

        public static string RankNeedR18On => GetResource(nameof(RankNeedR18On));

        public static string CannotFindSpecifiedCertificate => GetResource(nameof(CannotFindSpecifiedCertificate));

        public static string GifIllustrationHint => GetResource(nameof(GifIllustrationHint));

        public static string MangaIllustrationHintFormat => GetResource(nameof(MangaIllustrationHintFormat));

        public static string UserIdHintFormat => GetResource(nameof(UserIdHintFormat));

        public static string DownloadSingleIllustration => GetResource(nameof(DownloadSingleIllustration));

        public static string DownloadAllInCurrentList => GetResource(nameof(DownloadAllInCurrentList));

        public static string DownloadSpotlight => GetResource(nameof(DownloadSpotlight));

        public static string RetractSidebar => GetResource(nameof(RetractSidebar));

        public static string RestrictPolicy => GetResource(nameof(RestrictPolicy));

        public static string Private => GetResource(nameof(Private));

        public static string Public => GetResource(nameof(Public));

        public static string HomePage => GetResource(nameof(HomePage));

        public static string MyGallery => GetResource(nameof(MyGallery));

        public static string MyFollowing => GetResource(nameof(MyFollowing));

        public static string Spotlight => GetResource(nameof(Spotlight));

        public static string Recommend => GetResource(nameof(Recommend));

        public static string IllustRanking => GetResource(nameof(IllustRanking));

        public static string UserTrend => GetResource(nameof(UserTrend));

        public static string UserUpdate => GetResource(nameof(UserUpdate));

        public static string SearchImageBySource => GetResource(nameof(SearchImageBySource));

        public static string DownloadQueueAndHistory => GetResource(nameof(DownloadQueueAndHistory));

        public static string Setting => GetResource(nameof(Setting));

        public static string Logout => GetResource(nameof(Logout));

        public static string SearchHint => GetResource(nameof(SearchHint));

        public static string QueryUser => GetResource(nameof(QueryUser));

        public static string QuerySingleUser => GetResource(nameof(QuerySingleUser));

        public static string QuerySingleIllust => GetResource(nameof(QuerySingleIllust));

        public static string RecommendIllustratorTurnPage => GetResource(nameof(RecommendIllustratorTurnPage));

        public static string PixevalVersionFormat => GetResource(nameof(PixevalVersionFormat));

        public static string AboutPixeval => GetResource(nameof(AboutPixeval));

        public static string UserBrowserFollowCountHint => GetResource(nameof(UserBrowserFollowCountHint));

        public static string UserBrowserFollow => GetResource(nameof(UserBrowserFollow));

        public static string UserBrowserUnFollow => GetResource(nameof(UserBrowserUnFollow));

        public static string UserBrowserPrivateFollow => GetResource(nameof(UserBrowserPrivateFollow));

        public static string UserBrowserIllustSelector => GetResource(nameof(UserBrowserIllustSelector));

        public static string UserBrowserGallerySelector => GetResource(nameof(UserBrowserGallerySelector));

        public static string IllustBrowserIllustId => GetResource(nameof(IllustBrowserIllustId));

        public static string IllustBrowserTotalViews => GetResource(nameof(IllustBrowserTotalViews));

        public static string IllustBrowserTotalBookmarks => GetResource(nameof(IllustBrowserTotalBookmarks));

        public static string IllustBrowserResolution => GetResource(nameof(IllustBrowserResolution));

        public static string IllustBrowserUploadDate => GetResource(nameof(IllustBrowserUploadDate));

        public static string IllustBrowserIllustTag => GetResource(nameof(IllustBrowserIllustTag));

        public static string IllustBrowserSetWallpaper => GetResource(nameof(IllustBrowserSetWallpaper));

        public static string IllustBrowserShareLink => GetResource(nameof(IllustBrowserShareLink));

        public static string IllustBrowserViewInBrowser => GetResource(nameof(IllustBrowserViewInBrowser));

        public static string IllustBrowserDownload => GetResource(nameof(IllustBrowserDownload));

        public static string IllustBrowserPrivateBookmark => GetResource(nameof(IllustBrowserPrivateBookmark));

        public static string IllustBrowserBookmark => GetResource(nameof(IllustBrowserBookmark));

        public static string IllustBrowserRemoveBookmark => GetResource(nameof(IllustBrowserRemoveBookmark));

        public static string DownloadQueueShowDownloadIllust => GetResource(nameof(DownloadQueueShowDownloadIllust));

        public static string DownloadQueueRemoveFromDownloading =>
            GetResource(nameof(DownloadQueueRemoveFromDownloading));

        public static string DownloadQueueDownloading => GetResource(nameof(DownloadQueueDownloading));

        public static string DownloadQueueEmptyNotifier => GetResource(nameof(DownloadQueueEmptyNotifier));

        public static string DownloadQueueDownloaded => GetResource(nameof(DownloadQueueDownloaded));

        public static string EmailOrPasswordIsWrong => GetResource(nameof(EmailOrPasswordIsWrong));

        public static string Copy => GetResource(nameof(Copy));

        public static string ConditionBoxHint => GetResource(nameof(ConditionBoxHint));

        public static string PixevalSettings => GetResource(nameof(PixevalSettings));

        public static string SortByPopulation => GetResource(nameof(SortByPopulation));

        public static string TurnOffR18 => GetResource(nameof(TurnOffR18));

        public static string TurnOnIllustratorRecommend => GetResource(nameof(TurnOnIllustratorRecommend));

        public static string TurnOnDirectConnect => GetResource(nameof(TurnOnDirectConnect));

        public static string TagMatchOption => GetResource(nameof(TagMatchOption));

        public static string TurnOnCache => GetResource(nameof(TurnOnCache));

        public static string MemoryCachePolicy => GetResource(nameof(MemoryCachePolicy));

        public static string FileCachePolicy => GetResource(nameof(FileCachePolicy));

        public static string MinBookmarkRequired => GetResource(nameof(MinBookmarkRequired));

        public static string SearchPageCountHint => GetResource(nameof(SearchPageCountHint));

        public static string SearchPageStart => GetResource(nameof(SearchPageStart));

        public static string SpotlightSearchPageStart => GetResource(nameof(SpotlightSearchPageStart));

        public static string DownloadLocation => GetResource(nameof(DownloadLocation));

        public static string TagsToBeExclude => GetResource(nameof(TagsToBeExclude));

        public static string TagsToBeInclude => GetResource(nameof(TagsToBeInclude));

        public static string SearchPerPageCountHint => GetResource(nameof(SearchPerPageCountHint));

        public static string TurnOnWebR18 => GetResource(nameof(TurnOnWebR18));

        public static string SauceNaoFileLocationHint => GetResource(nameof(SauceNaoFileLocationHint));

        public static string SauceNaoUploadAndSearch => GetResource(nameof(SauceNaoUploadAndSearch));

        public static string SignIn => GetResource(nameof(SignIn));

        public static string SignInAccount => GetResource(nameof(SignInAccount));

        public static string SignInPassword => GetResource(nameof(SignInPassword));

        public static string SignInButtonText => GetResource(nameof(SignInButtonText));

        public static string SignInUpdatingSession => GetResource(nameof(SignInUpdatingSession));

        public static string BrowsingHistoryCount => GetResource(nameof(BrowsingHistoryCount));

        public static string DownloadQueueBrowsingHistory => GetResource(nameof(DownloadQueueBrowsingHistory));

        public static string DownloadQueueHistoryListIsEmpty => GetResource(nameof(DownloadQueueHistoryListIsEmpty));

        public static string PixevalUpdateAvailable => GetResource(nameof(PixevalUpdateAvailable));

        public static string PixevalUpdateAvailableTitle => GetResource(nameof(PixevalUpdateAvailableTitle));

        public static string ThisLoginSessionRequiresRecaptcha =>
            GetResource(nameof(ThisLoginSessionRequiresRecaptcha));

        public static string SupportMe => GetResource(nameof(SupportMe));
    }
}