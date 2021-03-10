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

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pixeval.Objects.I18n
{
    public static partial class AkaI18N
    {
        private static string _emptyEmailOrPasswordIsNotAllowed = GetResource(nameof(EmptyEmailOrPasswordIsNotAllowed));
        private static string _idDoNotExists = GetResource(nameof(IdDoNotExists));
        private static string _cannotFindUser = GetResource(nameof(CannotFindUser));
        private static string _inputIsEmpty = GetResource(nameof(InputIsEmpty));
        private static string _queryNotResponding = GetResource(nameof(QueryNotResponding));
        private static string _idIllegal = GetResource(nameof(IdIllegal));
        private static string _userIdIllegal = GetResource(nameof(UserIdIllegal));
        private static string _appApiAuthenticateTimeout = GetResource(nameof(AppApiAuthenticateTimeout));
        private static string _webApiAuthenticateTimeout = GetResource(nameof(WebApiAuthenticateTimeout));
        private static string _multiplePixevalInstanceDetected = GetResource(nameof(MultiplePixevalInstanceDetected));
        private static string _multiplePixevalInstanceDetectedTitle = GetResource(nameof(MultiplePixevalInstanceDetectedTitle));
        private static string _cppRedistributableRequired = GetResource(nameof(CppRedistributableRequired));
        private static string _cppRedistributableRequiredTitle = GetResource(nameof(CppRedistributableRequiredTitle));
        private static string _certificateInstallationIsRequired = GetResource(nameof(CertificateInstallationIsRequired));
        private static string _certificateInstallationIsRequiredTitle = GetResource(nameof(CertificateInstallationIsRequiredTitle));
        private static string _trendsAddIllust = GetResource(nameof(TrendsAddIllust));
        private static string _trendsAddBookmark = GetResource(nameof(TrendsAddBookmark));
        private static string _trendsAddFavorite = GetResource(nameof(TrendsAddFavorite));
        private static string _searchingTrends = GetResource(nameof(SearchingTrends));
        private static string _searchingUserUpdates = GetResource(nameof(SearchingUserUpdates));
        private static string _searchingGallery = GetResource(nameof(SearchingGallery));
        private static string _searchingRecommend = GetResource(nameof(SearchingRecommend));
        private static string _searchingFollower = GetResource(nameof(SearchingFollower));
        private static string _searchingSpotlight = GetResource(nameof(SearchingSpotlight));
        private static string _queuedDownload = GetResource(nameof(QueuedDownload));
        private static string _queuedAllToDownload = GetResource(nameof(QueuedAllToDownload));
        private static string _shareLinkCopiedToClipboard = GetResource(nameof(ShareLinkCopiedToClipboard));
        private static string _pathNotExist = GetResource(nameof(PathNotExist));
        private static string _sauceNaoFileCountLimit = GetResource(nameof(SauceNaoFileCountLimit));
        private static string _cannotFindResult = GetResource(nameof(CannotFindResult));
        private static string _pleaseSelectFile = GetResource(nameof(PleaseSelectFile));
        private static string _pleaseSelectLocation = GetResource(nameof(PleaseSelectLocation));
        private static string _cannotRetrieveContentLengthHeader = GetResource(nameof(CannotRetrieveContentLengthHeader));
        private static string _toggleR18OnSuccess = GetResource(nameof(ToggleR18OnSuccess));
        private static string _toggleR18OnFailed = GetResource(nameof(ToggleR18OnFailed));
        private static string _tryingToToggleR18Switch = GetResource(nameof(TryingToToggleR18Switch));
        private static string _rankOptionDay = GetResource(nameof(RankOptionDay));
        private static string _rankOptionWeek = GetResource(nameof(RankOptionWeek));
        private static string _rankOptionMonth = GetResource(nameof(RankOptionMonth));
        private static string _rankOptionDayMale = GetResource(nameof(RankOptionDayMale));
        private static string _rankOptionDayFemale = GetResource(nameof(RankOptionDayFemale));
        private static string _rankOptionDayManga = GetResource(nameof(RankOptionDayManga));
        private static string _rankOptionWeekManga = GetResource(nameof(RankOptionWeekManga));
        private static string _rankOptionWeekOriginal = GetResource(nameof(RankOptionWeekOriginal));
        private static string _rankOptionWeekRookie = GetResource(nameof(RankOptionWeekRookie));
        private static string _rankOptionDayR18 = GetResource(nameof(RankOptionDayR18));
        private static string _rankOptionDayMaleR18 = GetResource(nameof(RankOptionDayMaleR18));
        private static string _rankOptionDayFemaleR18 = GetResource(nameof(RankOptionDayFemaleR18));
        private static string _rankOptionWeekR18 = GetResource(nameof(RankOptionWeekR18));
        private static string _rankOptionWeekR18G = GetResource(nameof(RankOptionWeekR18G));
        private static string _rankDateCannotBeNull = GetResource(nameof(RankDateCannotBeNull));
        private static string _rankNeedR18On = GetResource(nameof(RankNeedR18On));
        private static string _cannotFindSpecifiedCertificate = GetResource(nameof(CannotFindSpecifiedCertificate));
        private static string _gifIllustrationHint = GetResource(nameof(GifIllustrationHint));
        private static string _mangaIllustrationHintFormat = GetResource(nameof(MangaIllustrationHintFormat));
        private static string _userIdHintFormat = GetResource(nameof(UserIdHintFormat));
        private static string _downloadSingleIllustration = GetResource(nameof(DownloadSingleIllustration));
        private static string _downloadAllInCurrentList = GetResource(nameof(DownloadAllInCurrentList));
        private static string _downloadSpotlight = GetResource(nameof(DownloadSpotlight));
        private static string _retractSidebar = GetResource(nameof(RetractSidebar));
        private static string _restrictPolicy = GetResource(nameof(RestrictPolicy));
        private static string _private = GetResource(nameof(Private));
        private static string _public = GetResource(nameof(Public));
        private static string _homePage = GetResource(nameof(HomePage));
        private static string _myGallery = GetResource(nameof(MyGallery));
        private static string _myFollowing = GetResource(nameof(MyFollowing));
        private static string _spotlight = GetResource(nameof(Spotlight));
        private static string _recommend = GetResource(nameof(Recommend));
        private static string _illustRanking = GetResource(nameof(IllustRanking));
        private static string _userTrend = GetResource(nameof(UserTrend));
        private static string _userUpdate = GetResource(nameof(UserUpdate));
        private static string _searchImageBySource = GetResource(nameof(SearchImageBySource));
        private static string _downloadQueueAndHistory = GetResource(nameof(DownloadQueueAndHistory));
        private static string _setting = GetResource(nameof(Setting));
        private static string _logout = GetResource(nameof(Logout));
        private static string _searchHint = GetResource(nameof(SearchHint));
        private static string _queryUser = GetResource(nameof(QueryUser));
        private static string _querySingleUser = GetResource(nameof(QuerySingleUser));
        private static string _querySingleIllust = GetResource(nameof(QuerySingleIllust));
        private static string _recommendIllustratorTurnPage = GetResource(nameof(RecommendIllustratorTurnPage));
        private static string _pixevalVersionFormat = GetResource(nameof(PixevalVersionFormat));
        private static string _aboutPixeval = GetResource(nameof(AboutPixeval));
        private static string _userBrowserFollowCountHint = GetResource(nameof(UserBrowserFollowCountHint));
        private static string _userBrowserFollow = GetResource(nameof(UserBrowserFollow));
        private static string _userBrowserUnFollow = GetResource(nameof(UserBrowserUnFollow));
        private static string _userBrowserPrivateFollow = GetResource(nameof(UserBrowserPrivateFollow));
        private static string _userBrowserIllustSelector = GetResource(nameof(UserBrowserIllustSelector));
        private static string _userBrowserGallerySelector = GetResource(nameof(UserBrowserGallerySelector));
        private static string _illustBrowserIllustId = GetResource(nameof(IllustBrowserIllustId));
        private static string _illustBrowserTotalViews = GetResource(nameof(IllustBrowserTotalViews));
        private static string _illustBrowserTotalBookmarks = GetResource(nameof(IllustBrowserTotalBookmarks));
        private static string _illustBrowserResolution = GetResource(nameof(IllustBrowserResolution));
        private static string _illustBrowserUploadDate = GetResource(nameof(IllustBrowserUploadDate));
        private static string _illustBrowserIllustTag = GetResource(nameof(IllustBrowserIllustTag));
        private static string _illustBrowserSetWallpaper = GetResource(nameof(IllustBrowserSetWallpaper));
        private static string _illustBrowserShareLink = GetResource(nameof(IllustBrowserShareLink));
        private static string _illustBrowserViewInBrowser = GetResource(nameof(IllustBrowserViewInBrowser));
        private static string _illustBrowserDownload = GetResource(nameof(IllustBrowserDownload));
        private static string _illustBrowserPrivateBookmark = GetResource(nameof(IllustBrowserPrivateBookmark));
        private static string _illustBrowserBookmark = GetResource(nameof(IllustBrowserBookmark));
        private static string _illustBrowserRemoveBookmark = GetResource(nameof(IllustBrowserRemoveBookmark));
        private static string _downloadQueueShowDownloadIllust = GetResource(nameof(DownloadQueueShowDownloadIllust));
        private static string _downloadQueueRemoveFromDownloading = GetResource(nameof(DownloadQueueRemoveFromDownloading));
        private static string _downloadQueueDownloading = GetResource(nameof(DownloadQueueDownloading));
        private static string _downloadQueueEmptyNotifier = GetResource(nameof(DownloadQueueEmptyNotifier));
        private static string _downloadQueueDownloaded = GetResource(nameof(DownloadQueueDownloaded));
        private static string _emailOrPasswordIsWrong = GetResource(nameof(EmailOrPasswordIsWrong));
        private static string _copy = GetResource(nameof(Copy));
        private static string _conditionBoxHint = GetResource(nameof(ConditionBoxHint));
        private static string _pixevalSettings = GetResource(nameof(PixevalSettings));
        private static string _sortByPopulation = GetResource(nameof(SortByPopulation));
        private static string _turnOffR18 = GetResource(nameof(TurnOffR18));
        private static string _turnOnIllustratorRecommend = GetResource(nameof(TurnOnIllustratorRecommend));
        private static string _turnOnDirectConnect = GetResource(nameof(TurnOnDirectConnect));
        private static string _tagMatchOption = GetResource(nameof(TagMatchOption));
        private static string _turnOnCache = GetResource(nameof(TurnOnCache));
        private static string _memoryCachePolicy = GetResource(nameof(MemoryCachePolicy));
        private static string _fileCachePolicy = GetResource(nameof(FileCachePolicy));
        private static string _minBookmarkRequired = GetResource(nameof(MinBookmarkRequired));
        private static string _searchPageCountHint = GetResource(nameof(SearchPageCountHint));
        private static string _searchPageStart = GetResource(nameof(SearchPageStart));
        private static string _spotlightSearchPageStart = GetResource(nameof(SpotlightSearchPageStart));
        private static string _downloadLocation = GetResource(nameof(DownloadLocation));
        private static string _tagsToBeExclude = GetResource(nameof(TagsToBeExclude));
        private static string _tagsToBeInclude = GetResource(nameof(TagsToBeInclude));
        private static string _searchPerPageCountHint = GetResource(nameof(SearchPerPageCountHint));
        private static string _turnOnWebR18 = GetResource(nameof(TurnOnWebR18));
        private static string _sauceNaoFileLocationHint = GetResource(nameof(SauceNaoFileLocationHint));
        private static string _sauceNaoUploadAndSearch = GetResource(nameof(SauceNaoUploadAndSearch));
        private static string _signIn = GetResource(nameof(SignIn));
        private static string _signInAccount = GetResource(nameof(SignInAccount));
        private static string _signInPassword = GetResource(nameof(SignInPassword));
        private static string _signInButtonText = GetResource(nameof(SignInButtonText));
        private static string _signInUpdatingSession = GetResource(nameof(SignInUpdatingSession));
        private static string _browsingHistoryCount = GetResource(nameof(BrowsingHistoryCount));
        private static string _downloadQueueBrowsingHistory = GetResource(nameof(DownloadQueueBrowsingHistory));
        private static string _downloadQueueHistoryListIsEmpty = GetResource(nameof(DownloadQueueHistoryListIsEmpty));
        private static string _pixevalUpdateAvailable = GetResource(nameof(PixevalUpdateAvailable));
        private static string _pixevalUpdateAvailableTitle = GetResource(nameof(PixevalUpdateAvailableTitle));
        private static string _thisLoginSessionRequiresRecaptcha = GetResource(nameof(ThisLoginSessionRequiresRecaptcha));
        private static string _supportMe = GetResource(nameof(SupportMe));
        private static string _downloadTo = GetResource(nameof(DownloadTo));
        private static string _userPreviewPopupFollow = GetResource(nameof(UserPreviewPopupFollow));
        private static string _userPreviewPopupUnFollow = GetResource(nameof(UserPreviewPopupUnFollow));
        private static string _selectCultureInfo = GetResource(nameof(SelectCultureInfo));
        private static string _createNewFolderWhenDownloadFromUser = GetResource(nameof(CreateNewFolderWhenDownloadFromUser));
        private static string _requiresWebCookie = GetResource(nameof(RequiresWebCookie));
        private static string _yes = GetResource(nameof(Yes));
        private static string _no = GetResource(nameof(No));
        private static string _warning = GetResource(nameof(Warning));
        private static string _fillWithWebCookie = GetResource(nameof(FillWithWebCookie));
        private static string _imageMirrorServerUrl = GetResource(nameof(ImageMirrorServerUrl));
        private static string _imageMirrorServerUrlHint = GetResource(nameof(ImageMirrorServerUrlHint));
        private static string _conditionBoxTooltip = GetResource(nameof(ConditionBoxTooltip));
        private static string _batchDownloadAcknowledgment = GetResource(nameof(BatchDownloadAcknowledgment));
        private static string _webView2DownloadIsRequired = GetResource(nameof(WebView2DownloadIsRequired));
        private static string _signInLoggingIn = GetResource(nameof(SignInLoggingIn));
        private static string _turnOffAnimation = GetResource(nameof(TurnOffAnimation));
        private static string _valueIsNotInteger = GetResource(nameof(ValueIsNotInteger));
        private static string _uriFormIncorrect = GetResource(nameof(UriFormIncorrect));
        private static string _searchPageCountToolTip = GetResource(nameof(SearchPageCountToolTip));
        private static string _searchPageStartToolTip = GetResource(nameof(SearchPageStartToolTip));
        private static string _spotlightSearchPageStartToolTip = GetResource(nameof(SpotlightSearchPageStartToolTip));
        private static string _minimumBookmarkToolTip = GetResource(nameof(MinimumBookmarkToolTip));
        private static string _sortByPopulationToolTip = GetResource(nameof(SortByPopulationToolTip));
        private static string _recommendIllustrationToolTip = GetResource(nameof(RecommendIllustrationToolTip));
        private static string _invalidDirectoryPath = GetResource(nameof(InvalidDirectoryPath));
        private static string _downloadPathSyntaxToolTip = GetResource(nameof(DownloadPathSyntaxToolTip));

        public static string EmptyEmailOrPasswordIsNotAllowed
        {
            get => _emptyEmailOrPasswordIsNotAllowed;
            set
            {
                _emptyEmailOrPasswordIsNotAllowed = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IdDoNotExists
        {
            get => _idDoNotExists;
            set
            {
                _idDoNotExists = value;
                OnStaticPropertyChanged();
            }
        }

        public static string CannotFindUser
        {
            get => _cannotFindUser;
            set
            {
                _cannotFindUser = value;
                OnStaticPropertyChanged();
            }
        }

        public static string InputIsEmpty
        {
            get => _inputIsEmpty;
            set
            {
                _inputIsEmpty = value;
                OnStaticPropertyChanged();
            }
        }

        public static string QueryNotResponding
        {
            get => _queryNotResponding;
            set
            {
                _queryNotResponding = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IdIllegal
        {
            get => _idIllegal;
            set
            {
                _idIllegal = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserIdIllegal
        {
            get => _userIdIllegal;
            set
            {
                _userIdIllegal = value;
                OnStaticPropertyChanged();
            }
        }

        public static string AppApiAuthenticateTimeout
        {
            get => _appApiAuthenticateTimeout;
            set
            {
                _appApiAuthenticateTimeout = value;
                OnStaticPropertyChanged();
            }
        }

        public static string WebApiAuthenticateTimeout
        {
            get => _webApiAuthenticateTimeout;
            set
            {
                _webApiAuthenticateTimeout = value;
                OnStaticPropertyChanged();
            }
        }

        public static string MultiplePixevalInstanceDetected
        {
            get => _multiplePixevalInstanceDetected;
            set
            {
                _multiplePixevalInstanceDetected = value;
                OnStaticPropertyChanged();
            }
        }

        public static string MultiplePixevalInstanceDetectedTitle
        {
            get => _multiplePixevalInstanceDetectedTitle;
            set
            {
                _multiplePixevalInstanceDetectedTitle = value;
                OnStaticPropertyChanged();
            }
        }

        public static string CppRedistributableRequired
        {
            get => _cppRedistributableRequired;
            set
            {
                _cppRedistributableRequired = value;
                OnStaticPropertyChanged();
            }
        }

        public static string CppRedistributableRequiredTitle
        {
            get => _cppRedistributableRequiredTitle;
            set
            {
                _cppRedistributableRequiredTitle = value;
                OnStaticPropertyChanged();
            }
        }

        public static string CertificateInstallationIsRequired
        {
            get => _certificateInstallationIsRequired;
            set
            {
                _certificateInstallationIsRequired = value;
                OnStaticPropertyChanged();
            }
        }

        public static string CertificateInstallationIsRequiredTitle
        {
            get => _certificateInstallationIsRequiredTitle;
            set
            {
                _certificateInstallationIsRequiredTitle = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TrendsAddIllust
        {
            get => _trendsAddIllust;
            set
            {
                _trendsAddIllust = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TrendsAddBookmark
        {
            get => _trendsAddBookmark;
            set
            {
                _trendsAddBookmark = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TrendsAddFavorite
        {
            get => _trendsAddFavorite;
            set
            {
                _trendsAddFavorite = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchingTrends
        {
            get => _searchingTrends;
            set
            {
                _searchingTrends = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchingUserUpdates
        {
            get => _searchingUserUpdates;
            set
            {
                _searchingUserUpdates = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchingGallery
        {
            get => _searchingGallery;
            set
            {
                _searchingGallery = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchingRecommend
        {
            get => _searchingRecommend;
            set
            {
                _searchingRecommend = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchingFollower
        {
            get => _searchingFollower;
            set
            {
                _searchingFollower = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchingSpotlight
        {
            get => _searchingSpotlight;
            set
            {
                _searchingSpotlight = value;
                OnStaticPropertyChanged();
            }
        }

        public static string QueuedDownload
        {
            get => _queuedDownload;
            set
            {
                _queuedDownload = value;
                OnStaticPropertyChanged();
            }
        }

        public static string QueuedAllToDownload
        {
            get => _queuedAllToDownload;
            set
            {
                _queuedAllToDownload = value;
                OnStaticPropertyChanged();
            }
        }

        public static string ShareLinkCopiedToClipboard
        {
            get => _shareLinkCopiedToClipboard;
            set
            {
                _shareLinkCopiedToClipboard = value;
                OnStaticPropertyChanged();
            }
        }

        public static string PathNotExist
        {
            get => _pathNotExist;
            set
            {
                _pathNotExist = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SauceNaoFileCountLimit
        {
            get => _sauceNaoFileCountLimit;
            set
            {
                _sauceNaoFileCountLimit = value;
                OnStaticPropertyChanged();
            }
        }

        public static string CannotFindResult
        {
            get => _cannotFindResult;
            set
            {
                _cannotFindResult = value;
                OnStaticPropertyChanged();
            }
        }

        public static string PleaseSelectFile
        {
            get => _pleaseSelectFile;
            set
            {
                _pleaseSelectFile = value;
                OnStaticPropertyChanged();
            }
        }

        public static string PleaseSelectLocation
        {
            get => _pleaseSelectLocation;
            set
            {
                _pleaseSelectLocation = value;
                OnStaticPropertyChanged();
            }
        }

        public static string CannotRetrieveContentLengthHeader
        {
            get => _cannotRetrieveContentLengthHeader;
            set
            {
                _cannotRetrieveContentLengthHeader = value;
                OnStaticPropertyChanged();
            }
        }

        public static string ToggleR18OnSuccess
        {
            get => _toggleR18OnSuccess;
            set
            {
                _toggleR18OnSuccess = value;
                OnStaticPropertyChanged();
            }
        }

        public static string ToggleR18OnFailed
        {
            get => _toggleR18OnFailed;
            set
            {
                _toggleR18OnFailed = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TryingToToggleR18Switch
        {
            get => _tryingToToggleR18Switch;
            set
            {
                _tryingToToggleR18Switch = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionDay
        {
            get => _rankOptionDay;
            set
            {
                _rankOptionDay = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionWeek
        {
            get => _rankOptionWeek;
            set
            {
                _rankOptionWeek = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionMonth
        {
            get => _rankOptionMonth;
            set
            {
                _rankOptionMonth = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionDayMale
        {
            get => _rankOptionDayMale;
            set
            {
                _rankOptionDayMale = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionDayFemale
        {
            get => _rankOptionDayFemale;
            set
            {
                _rankOptionDayFemale = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionDayManga
        {
            get => _rankOptionDayManga;
            set
            {
                _rankOptionDayManga = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionWeekManga
        {
            get => _rankOptionWeekManga;
            set
            {
                _rankOptionWeekManga = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionWeekOriginal
        {
            get => _rankOptionWeekOriginal;
            set
            {
                _rankOptionWeekOriginal = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionWeekRookie
        {
            get => _rankOptionWeekRookie;
            set
            {
                _rankOptionWeekRookie = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionDayR18
        {
            get => _rankOptionDayR18;
            set
            {
                _rankOptionDayR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionDayMaleR18
        {
            get => _rankOptionDayMaleR18;
            set
            {
                _rankOptionDayMaleR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionDayFemaleR18
        {
            get => _rankOptionDayFemaleR18;
            set
            {
                _rankOptionDayFemaleR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionWeekR18
        {
            get => _rankOptionWeekR18;
            set
            {
                _rankOptionWeekR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankOptionWeekR18G
        {
            get => _rankOptionWeekR18G;
            set
            {
                _rankOptionWeekR18G = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankDateCannotBeNull
        {
            get => _rankDateCannotBeNull;
            set
            {
                _rankDateCannotBeNull = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RankNeedR18On
        {
            get => _rankNeedR18On;
            set
            {
                _rankNeedR18On = value;
                OnStaticPropertyChanged();
            }
        }

        public static string CannotFindSpecifiedCertificate
        {
            get => _cannotFindSpecifiedCertificate;
            set
            {
                _cannotFindSpecifiedCertificate = value;
                OnStaticPropertyChanged();
            }
        }

        public static string GifIllustrationHint
        {
            get => _gifIllustrationHint;
            set
            {
                _gifIllustrationHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string MangaIllustrationHintFormat
        {
            get => _mangaIllustrationHintFormat;
            set
            {
                _mangaIllustrationHintFormat = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserIdHintFormat
        {
            get => _userIdHintFormat;
            set
            {
                _userIdHintFormat = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadSingleIllustration
        {
            get => _downloadSingleIllustration;
            set
            {
                _downloadSingleIllustration = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadAllInCurrentList
        {
            get => _downloadAllInCurrentList;
            set
            {
                _downloadAllInCurrentList = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadSpotlight
        {
            get => _downloadSpotlight;
            set
            {
                _downloadSpotlight = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RetractSidebar
        {
            get => _retractSidebar;
            set
            {
                _retractSidebar = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RestrictPolicy
        {
            get => _restrictPolicy;
            set
            {
                _restrictPolicy = value;
                OnStaticPropertyChanged();
            }
        }

        public static string Private
        {
            get => _private;
            set
            {
                _private = value;
                OnStaticPropertyChanged();
            }
        }

        public static string Public
        {
            get => _public;
            set
            {
                _public = value;
                OnStaticPropertyChanged();
            }
        }

        public static string HomePage
        {
            get => _homePage;
            set
            {
                _homePage = value;
                OnStaticPropertyChanged();
            }
        }

        public static string MyGallery
        {
            get => _myGallery;
            set
            {
                _myGallery = value;
                OnStaticPropertyChanged();
            }
        }

        public static string MyFollowing
        {
            get => _myFollowing;
            set
            {
                _myFollowing = value;
                OnStaticPropertyChanged();
            }
        }

        public static string Spotlight
        {
            get => _spotlight;
            set
            {
                _spotlight = value;
                OnStaticPropertyChanged();
            }
        }

        public static string Recommend
        {
            get => _recommend;
            set
            {
                _recommend = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustRanking
        {
            get => _illustRanking;
            set
            {
                _illustRanking = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserTrend
        {
            get => _userTrend;
            set
            {
                _userTrend = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserUpdate
        {
            get => _userUpdate;
            set
            {
                _userUpdate = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchImageBySource
        {
            get => _searchImageBySource;
            set
            {
                _searchImageBySource = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadQueueAndHistory
        {
            get => _downloadQueueAndHistory;
            set
            {
                _downloadQueueAndHistory = value;
                OnStaticPropertyChanged();
            }
        }

        public static string Setting
        {
            get => _setting;
            set
            {
                _setting = value;
                OnStaticPropertyChanged();
            }
        }

        public static string Logout
        {
            get => _logout;
            set
            {
                _logout = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchHint
        {
            get => _searchHint;
            set
            {
                _searchHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string QueryUser
        {
            get => _queryUser;
            set
            {
                _queryUser = value;
                OnStaticPropertyChanged();
            }
        }

        public static string QuerySingleUser
        {
            get => _querySingleUser;
            set
            {
                _querySingleUser = value;
                OnStaticPropertyChanged();
            }
        }

        public static string QuerySingleIllust
        {
            get => _querySingleIllust;
            set
            {
                _querySingleIllust = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RecommendIllustratorTurnPage
        {
            get => _recommendIllustratorTurnPage;
            set
            {
                _recommendIllustratorTurnPage = value;
                OnStaticPropertyChanged();
            }
        }

        public static string PixevalVersionFormat
        {
            get => _pixevalVersionFormat;
            set
            {
                _pixevalVersionFormat = value;
                OnStaticPropertyChanged();
            }
        }

        public static string AboutPixeval
        {
            get => _aboutPixeval;
            set
            {
                _aboutPixeval = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserBrowserFollowCountHint
        {
            get => _userBrowserFollowCountHint;
            set
            {
                _userBrowserFollowCountHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserBrowserFollow
        {
            get => _userBrowserFollow;
            set
            {
                _userBrowserFollow = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserBrowserUnFollow
        {
            get => _userBrowserUnFollow;
            set
            {
                _userBrowserUnFollow = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserBrowserPrivateFollow
        {
            get => _userBrowserPrivateFollow;
            set
            {
                _userBrowserPrivateFollow = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserBrowserIllustSelector
        {
            get => _userBrowserIllustSelector;
            set
            {
                _userBrowserIllustSelector = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserBrowserGallerySelector
        {
            get => _userBrowserGallerySelector;
            set
            {
                _userBrowserGallerySelector = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserIllustId
        {
            get => _illustBrowserIllustId;
            set
            {
                _illustBrowserIllustId = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserTotalViews
        {
            get => _illustBrowserTotalViews;
            set
            {
                _illustBrowserTotalViews = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserTotalBookmarks
        {
            get => _illustBrowserTotalBookmarks;
            set
            {
                _illustBrowserTotalBookmarks = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserResolution
        {
            get => _illustBrowserResolution;
            set
            {
                _illustBrowserResolution = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserUploadDate
        {
            get => _illustBrowserUploadDate;
            set
            {
                _illustBrowserUploadDate = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserIllustTag
        {
            get => _illustBrowserIllustTag;
            set
            {
                _illustBrowserIllustTag = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserSetWallpaper
        {
            get => _illustBrowserSetWallpaper;
            set
            {
                _illustBrowserSetWallpaper = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserShareLink
        {
            get => _illustBrowserShareLink;
            set
            {
                _illustBrowserShareLink = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserViewInBrowser
        {
            get => _illustBrowserViewInBrowser;
            set
            {
                _illustBrowserViewInBrowser = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserDownload
        {
            get => _illustBrowserDownload;
            set
            {
                _illustBrowserDownload = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserPrivateBookmark
        {
            get => _illustBrowserPrivateBookmark;
            set
            {
                _illustBrowserPrivateBookmark = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserBookmark
        {
            get => _illustBrowserBookmark;
            set
            {
                _illustBrowserBookmark = value;
                OnStaticPropertyChanged();
            }
        }

        public static string IllustBrowserRemoveBookmark
        {
            get => _illustBrowserRemoveBookmark;
            set
            {
                _illustBrowserRemoveBookmark = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadQueueShowDownloadIllust
        {
            get => _downloadQueueShowDownloadIllust;
            set
            {
                _downloadQueueShowDownloadIllust = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadQueueRemoveFromDownloading
        {
            get => _downloadQueueRemoveFromDownloading;
            set
            {
                _downloadQueueRemoveFromDownloading = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadQueueDownloading
        {
            get => _downloadQueueDownloading;
            set
            {
                _downloadQueueDownloading = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadQueueEmptyNotifier
        {
            get => _downloadQueueEmptyNotifier;
            set
            {
                _downloadQueueEmptyNotifier = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadQueueDownloaded
        {
            get => _downloadQueueDownloaded;
            set
            {
                _downloadQueueDownloaded = value;
                OnStaticPropertyChanged();
            }
        }

        public static string EmailOrPasswordIsWrong
        {
            get => _emailOrPasswordIsWrong;
            set
            {
                _emailOrPasswordIsWrong = value;
                OnStaticPropertyChanged();
            }
        }

        public static string Copy
        {
            get => _copy;
            set
            {
                _copy = value;
                OnStaticPropertyChanged();
            }
        }

        public static string ConditionBoxHint
        {
            get => _conditionBoxHint;
            set
            {
                _conditionBoxHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string PixevalSettings
        {
            get => _pixevalSettings;
            set
            {
                _pixevalSettings = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SortByPopulation
        {
            get => _sortByPopulation;
            set
            {
                _sortByPopulation = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TurnOffR18
        {
            get => _turnOffR18;
            set
            {
                _turnOffR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TurnOnIllustratorRecommend
        {
            get => _turnOnIllustratorRecommend;
            set
            {
                _turnOnIllustratorRecommend = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TurnOnDirectConnect
        {
            get => _turnOnDirectConnect;
            set
            {
                _turnOnDirectConnect = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TagMatchOption
        {
            get => _tagMatchOption;
            set
            {
                _tagMatchOption = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TurnOnCache
        {
            get => _turnOnCache;
            set
            {
                _turnOnCache = value;
                OnStaticPropertyChanged();
            }
        }

        public static string MemoryCachePolicy
        {
            get => _memoryCachePolicy;
            set
            {
                _memoryCachePolicy = value;
                OnStaticPropertyChanged();
            }
        }

        public static string FileCachePolicy
        {
            get => _fileCachePolicy;
            set
            {
                _fileCachePolicy = value;
                OnStaticPropertyChanged();
            }
        }

        public static string MinBookmarkRequired
        {
            get => _minBookmarkRequired;
            set
            {
                _minBookmarkRequired = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchPageCountHint
        {
            get => _searchPageCountHint;
            set
            {
                _searchPageCountHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchPageStart
        {
            get => _searchPageStart;
            set
            {
                _searchPageStart = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SpotlightSearchPageStart
        {
            get => _spotlightSearchPageStart;
            set
            {
                _spotlightSearchPageStart = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadLocation
        {
            get => _downloadLocation;
            set
            {
                _downloadLocation = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TagsToBeExclude
        {
            get => _tagsToBeExclude;
            set
            {
                _tagsToBeExclude = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TagsToBeInclude
        {
            get => _tagsToBeInclude;
            set
            {
                _tagsToBeInclude = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchPerPageCountHint
        {
            get => _searchPerPageCountHint;
            set
            {
                _searchPerPageCountHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TurnOnWebR18
        {
            get => _turnOnWebR18;
            set
            {
                _turnOnWebR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SauceNaoFileLocationHint
        {
            get => _sauceNaoFileLocationHint;
            set
            {
                _sauceNaoFileLocationHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SauceNaoUploadAndSearch
        {
            get => _sauceNaoUploadAndSearch;
            set
            {
                _sauceNaoUploadAndSearch = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SignIn
        {
            get => _signIn;
            set
            {
                _signIn = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SignInAccount
        {
            get => _signInAccount;
            set
            {
                _signInAccount = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SignInPassword
        {
            get => _signInPassword;
            set
            {
                _signInPassword = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SignInButtonText
        {
            get => _signInButtonText;
            set
            {
                _signInButtonText = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SignInUpdatingSession
        {
            get => _signInUpdatingSession;
            set
            {
                _signInUpdatingSession = value;
                OnStaticPropertyChanged();
            }
        }

        public static string BrowsingHistoryCount
        {
            get => _browsingHistoryCount;
            set
            {
                _browsingHistoryCount = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadQueueBrowsingHistory
        {
            get => _downloadQueueBrowsingHistory;
            set
            {
                _downloadQueueBrowsingHistory = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadQueueHistoryListIsEmpty
        {
            get => _downloadQueueHistoryListIsEmpty;
            set
            {
                _downloadQueueHistoryListIsEmpty = value;
                OnStaticPropertyChanged();
            }
        }

        public static string PixevalUpdateAvailable
        {
            get => _pixevalUpdateAvailable;
            set
            {
                _pixevalUpdateAvailable = value;
                OnStaticPropertyChanged();
            }
        }

        public static string PixevalUpdateAvailableTitle
        {
            get => _pixevalUpdateAvailableTitle;
            set
            {
                _pixevalUpdateAvailableTitle = value;
                OnStaticPropertyChanged();
            }
        }

        public static string ThisLoginSessionRequiresRecaptcha
        {
            get => _thisLoginSessionRequiresRecaptcha;
            set
            {
                _thisLoginSessionRequiresRecaptcha = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SupportMe
        {
            get => _supportMe;
            set
            {
                _supportMe = value;
                OnStaticPropertyChanged();
            }
        }

        public static string DownloadTo
        {
            get => _downloadTo;
            set
            {
                _downloadTo = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserPreviewPopupFollow
        {
            get => _userPreviewPopupFollow;
            set
            {
                _userPreviewPopupFollow = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UserPreviewPopupUnFollow
        {
            get => _userPreviewPopupUnFollow;
            set
            {
                _userPreviewPopupUnFollow = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SelectCultureInfo
        {
            get => _selectCultureInfo;
            set
            {
                _selectCultureInfo = value;
                OnStaticPropertyChanged();
            }
        }

        public static string CreateNewFolderWhenDownloadFromUser
        {
            get => _createNewFolderWhenDownloadFromUser;
            set
            {
                _createNewFolderWhenDownloadFromUser = value;
                OnStaticPropertyChanged();
            }
        }

        public static string RequiresWebCookie
        {
            get => _requiresWebCookie;
            set
            {
                _requiresWebCookie = value;
                OnStaticPropertyChanged();
            }
        }

        public static string Yes
        {
            get => _yes;
            set
            {
                _yes = value;
                OnStaticPropertyChanged();
            }
        }

        public static string No
        {
            get => _no;
            set
            {
                _no = value;
                OnStaticPropertyChanged();
            }
        }

        public static string Warning
        {
            get => _warning;
            set
            {
                _warning = value;
                OnStaticPropertyChanged();
            }
        }

        public static string FillWithWebCookie
        {
            get => _fillWithWebCookie;
            set
            {
                _fillWithWebCookie = value;
                OnStaticPropertyChanged();
            }
        }

        public static string ImageMirrorServerUrl
        {
            get => _imageMirrorServerUrl;
            set
            {
                _imageMirrorServerUrl = value;
                OnStaticPropertyChanged();
            }
        }

        public static string ImageMirrorServerUrlHint
        {
            get => _imageMirrorServerUrlHint;
            set
            {
                _imageMirrorServerUrlHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string ConditionBoxTooltip
        {
            get => _conditionBoxTooltip;
            set
            {
                _conditionBoxTooltip = value;
                OnStaticPropertyChanged();
            }
        }

        public static string BatchDownloadAcknowledgment
        {
            get => _batchDownloadAcknowledgment;
            set
            {
                _batchDownloadAcknowledgment = value;
                OnStaticPropertyChanged();
            }
        }

        public static string WebView2DownloadIsRequired
        {
            get => _webView2DownloadIsRequired;
            set
            {
                _webView2DownloadIsRequired = value;
                OnStaticPropertyChanged();
            }
        }
        
        public static string SignInLoggingIn
        {
            get => _signInLoggingIn;
            set
            {
                _signInLoggingIn = value;
                OnStaticPropertyChanged();
            }
        }

        public static string TurnOffAnimation
        {
            get => _turnOffAnimation;
            set
            {
                _turnOffAnimation = value;
                OnStaticPropertyChanged();
            }
        }

        public static string ValueIsNotInteger
        {
            get => _valueIsNotInteger;
            set
            {
                _valueIsNotInteger = value;
                OnStaticPropertyChanged();
            }
        }

        public static string UriFormIncorrect
        {
            get => _uriFormIncorrect;
            set
            {
                _uriFormIncorrect = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SearchPageCountToolTip
        {
            get => _searchPageCountToolTip;
            set
            {
                _searchPageCountToolTip = value;
                OnStaticPropertyChanged();
            }
        }
        
        public static string SearchPageStartToolTip
        {
            get => _searchPageStartToolTip;
            set
            {
                _searchPageStartToolTip = value;
                OnStaticPropertyChanged();
            }
        }

        public static string SpotlightSearchPageStartToolTip
        {
            get => _spotlightSearchPageStartToolTip;
            set
            {
                _spotlightSearchPageStartToolTip = value;
                OnStaticPropertyChanged();
            }
        } 
        
        public static string MinimumBookmarkToolTip
        {
            get => _minimumBookmarkToolTip;
            set
            {
                _minimumBookmarkToolTip = value;
                OnStaticPropertyChanged();
            }
        }
        
        public static string SortByPopulationToolTip
        {
            get => _sortByPopulationToolTip;
            set
            {
                _sortByPopulationToolTip = value;
                OnStaticPropertyChanged();
            }
        }
        
        public static string RecommendIllustrationToolTip
        {
            get => _recommendIllustrationToolTip;
            set
            {
                _recommendIllustrationToolTip = value;
                OnStaticPropertyChanged();
            }
        }
        
        public static string InvalidDirectoryPath
        {
            get => _invalidDirectoryPath;
            set
            {
                _invalidDirectoryPath = value;
                OnStaticPropertyChanged();
            }
        }
        
        public static string DownloadPathSyntaxToolTip
        {
            get => _downloadPathSyntaxToolTip;
            set
            {
                _downloadPathSyntaxToolTip = value;
                OnStaticPropertyChanged();
            }
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private static void OnStaticPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}