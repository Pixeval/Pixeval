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
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private static void OnStaticPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public static string emptyEmailOrPasswordIsNotAllowed = GetResource(nameof(EmptyEmailOrPasswordIsNotAllowed));

        public static string EmptyEmailOrPasswordIsNotAllowed
        {
            get => emptyEmailOrPasswordIsNotAllowed;
            set
            {
                emptyEmailOrPasswordIsNotAllowed = value;
                OnStaticPropertyChanged();
            }
        }

        public static string idDoNotExists = GetResource(nameof(IdDoNotExists));

        public static string IdDoNotExists
        {
            get => idDoNotExists;
            set
            {
                idDoNotExists = value;
                OnStaticPropertyChanged();
            }
        }

        public static string cannotFindUser = GetResource(nameof(CannotFindUser));

        public static string CannotFindUser
        {
            get => cannotFindUser;
            set
            {
                cannotFindUser = value;
                OnStaticPropertyChanged();
            }
        }

        public static string inputIsEmpty = GetResource(nameof(InputIsEmpty));

        public static string InputIsEmpty
        {
            get => inputIsEmpty;
            set
            {
                inputIsEmpty = value;
                OnStaticPropertyChanged();
            }
        }

        public static string queryNotResponding = GetResource(nameof(QueryNotResponding));

        public static string QueryNotResponding
        {
            get => queryNotResponding;
            set
            {
                queryNotResponding = value;
                OnStaticPropertyChanged();
            }
        }

        public static string idIllegal = GetResource(nameof(IdIllegal));

        public static string IdIllegal
        {
            get => idIllegal;
            set
            {
                idIllegal = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userIdIllegal = GetResource(nameof(UserIdIllegal));

        public static string UserIdIllegal
        {
            get => userIdIllegal;
            set
            {
                userIdIllegal = value;
                OnStaticPropertyChanged();
            }
        }

        public static string appApiAuthenticateTimeout = GetResource(nameof(AppApiAuthenticateTimeout));

        public static string AppApiAuthenticateTimeout
        {
            get => appApiAuthenticateTimeout;
            set
            {
                appApiAuthenticateTimeout = value;
                OnStaticPropertyChanged();
            }
        }

        public static string webApiAuthenticateTimeout = GetResource(nameof(WebApiAuthenticateTimeout));

        public static string WebApiAuthenticateTimeout
        {
            get => webApiAuthenticateTimeout;
            set
            {
                webApiAuthenticateTimeout = value;
                OnStaticPropertyChanged();
            }
        }

        public static string multiplePixevalInstanceDetected = GetResource(nameof(MultiplePixevalInstanceDetected));

        public static string MultiplePixevalInstanceDetected
        {
            get => multiplePixevalInstanceDetected;
            set
            {
                multiplePixevalInstanceDetected = value;
                OnStaticPropertyChanged();
            }
        }

        public static string multiplePixevalInstanceDetectedTitle = GetResource(nameof(MultiplePixevalInstanceDetectedTitle));

        public static string MultiplePixevalInstanceDetectedTitle
        {
            get => multiplePixevalInstanceDetectedTitle;
            set
            {
                multiplePixevalInstanceDetectedTitle = value;
                OnStaticPropertyChanged();
            }
        }

        public static string cppRedistributableRequired = GetResource(nameof(CppRedistributableRequired));

        public static string CppRedistributableRequired
        {
            get => cppRedistributableRequired;
            set
            {
                cppRedistributableRequired = value;
                OnStaticPropertyChanged();
            }
        }

        public static string cppRedistributableRequiredTitle = GetResource(nameof(CppRedistributableRequiredTitle));

        public static string CppRedistributableRequiredTitle
        {
            get => cppRedistributableRequiredTitle;
            set
            {
                cppRedistributableRequiredTitle = value;
                OnStaticPropertyChanged();
            }
        }

        public static string certificateInstallationIsRequired = GetResource(nameof(CertificateInstallationIsRequired));

        public static string CertificateInstallationIsRequired
        {
            get => certificateInstallationIsRequired;
            set
            {
                certificateInstallationIsRequired = value;
                OnStaticPropertyChanged();
            }
        }

        public static string certificateInstallationIsRequiredTitle = GetResource(nameof(CertificateInstallationIsRequiredTitle));

        public static string CertificateInstallationIsRequiredTitle
        {
            get => certificateInstallationIsRequiredTitle;
            set
            {
                certificateInstallationIsRequiredTitle = value;
                OnStaticPropertyChanged();
            }
        }

        public static string trendsAddIllust = GetResource(nameof(TrendsAddIllust));

        public static string TrendsAddIllust
        {
            get => trendsAddIllust;
            set
            {
                trendsAddIllust = value;
                OnStaticPropertyChanged();
            }
        }

        public static string trendsAddBookmark = GetResource(nameof(TrendsAddBookmark));

        public static string TrendsAddBookmark
        {
            get => trendsAddBookmark;
            set
            {
                trendsAddBookmark = value;
                OnStaticPropertyChanged();
            }
        }

        public static string trendsAddFavorite = GetResource(nameof(TrendsAddFavorite));

        public static string TrendsAddFavorite
        {
            get => trendsAddFavorite;
            set
            {
                trendsAddFavorite = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchingTrends = GetResource(nameof(SearchingTrends));

        public static string SearchingTrends
        {
            get => searchingTrends;
            set
            {
                searchingTrends = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchingUserUpdates = GetResource(nameof(SearchingUserUpdates));

        public static string SearchingUserUpdates
        {
            get => searchingUserUpdates;
            set
            {
                searchingUserUpdates = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchingGallery = GetResource(nameof(SearchingGallery));

        public static string SearchingGallery
        {
            get => searchingGallery;
            set
            {
                searchingGallery = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchingRecommend = GetResource(nameof(SearchingRecommend));

        public static string SearchingRecommend
        {
            get => searchingRecommend;
            set
            {
                searchingRecommend = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchingFollower = GetResource(nameof(SearchingFollower));

        public static string SearchingFollower
        {
            get => searchingFollower;
            set
            {
                searchingFollower = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchingSpotlight = GetResource(nameof(SearchingSpotlight));

        public static string SearchingSpotlight
        {
            get => searchingSpotlight;
            set
            {
                searchingSpotlight = value;
                OnStaticPropertyChanged();
            }
        }

        public static string queuedDownload = GetResource(nameof(QueuedDownload));

        public static string QueuedDownload
        {
            get => queuedDownload;
            set
            {
                queuedDownload = value;
                OnStaticPropertyChanged();
            }
        }

        public static string queuedAllToDownload = GetResource(nameof(QueuedAllToDownload));

        public static string QueuedAllToDownload
        {
            get => queuedAllToDownload;
            set
            {
                queuedAllToDownload = value;
                OnStaticPropertyChanged();
            }
        }

        public static string shareLinkCopiedToClipboard = GetResource(nameof(ShareLinkCopiedToClipboard));

        public static string ShareLinkCopiedToClipboard
        {
            get => shareLinkCopiedToClipboard;
            set
            {
                shareLinkCopiedToClipboard = value;
                OnStaticPropertyChanged();
            }
        }

        public static string pathNotExist = GetResource(nameof(PathNotExist));

        public static string PathNotExist
        {
            get => pathNotExist;
            set
            {
                pathNotExist = value;
                OnStaticPropertyChanged();
            }
        }

        public static string sauceNaoFileCountLimit = GetResource(nameof(SauceNaoFileCountLimit));

        public static string SauceNaoFileCountLimit
        {
            get => sauceNaoFileCountLimit;
            set
            {
                sauceNaoFileCountLimit = value;
                OnStaticPropertyChanged();
            }
        }

        public static string cannotFindResult = GetResource(nameof(CannotFindResult));

        public static string CannotFindResult
        {
            get => cannotFindResult;
            set
            {
                cannotFindResult = value;
                OnStaticPropertyChanged();
            }
        }

        public static string pleaseSelectFile = GetResource(nameof(PleaseSelectFile));

        public static string PleaseSelectFile
        {
            get => pleaseSelectFile;
            set
            {
                pleaseSelectFile = value;
                OnStaticPropertyChanged();
            }
        }

        public static string pleaseSelectLocation = GetResource(nameof(PleaseSelectLocation));

        public static string PleaseSelectLocation
        {
            get => pleaseSelectLocation;
            set
            {
                pleaseSelectLocation = value;
                OnStaticPropertyChanged();
            }
        }

        public static string cannotRetrieveContentLengthHeader = GetResource(nameof(CannotRetrieveContentLengthHeader));

        public static string CannotRetrieveContentLengthHeader
        {
            get => cannotRetrieveContentLengthHeader;
            set
            {
                cannotRetrieveContentLengthHeader = value;
                OnStaticPropertyChanged();
            }
        }

        public static string toggleR18OnSuccess = GetResource(nameof(ToggleR18OnSuccess));

        public static string ToggleR18OnSuccess
        {
            get => toggleR18OnSuccess;
            set
            {
                toggleR18OnSuccess = value;
                OnStaticPropertyChanged();
            }
        }

        public static string toggleR18OnFailed = GetResource(nameof(ToggleR18OnFailed));

        public static string ToggleR18OnFailed
        {
            get => toggleR18OnFailed;
            set
            {
                toggleR18OnFailed = value;
                OnStaticPropertyChanged();
            }
        }

        public static string tryingToToggleR18Switch = GetResource(nameof(TryingToToggleR18Switch));

        public static string TryingToToggleR18Switch
        {
            get => tryingToToggleR18Switch;
            set
            {
                tryingToToggleR18Switch = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionDay = GetResource(nameof(RankOptionDay));

        public static string RankOptionDay
        {
            get => rankOptionDay;
            set
            {
                rankOptionDay = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionWeek = GetResource(nameof(RankOptionWeek));

        public static string RankOptionWeek
        {
            get => rankOptionWeek;
            set
            {
                rankOptionWeek = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionMonth = GetResource(nameof(RankOptionMonth));

        public static string RankOptionMonth
        {
            get => rankOptionMonth;
            set
            {
                rankOptionMonth = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionDayMale = GetResource(nameof(RankOptionDayMale));

        public static string RankOptionDayMale
        {
            get => rankOptionDayMale;
            set
            {
                rankOptionDayMale = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionDayFemale = GetResource(nameof(RankOptionDayFemale));

        public static string RankOptionDayFemale
        {
            get => rankOptionDayFemale;
            set
            {
                rankOptionDayFemale = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionDayManga = GetResource(nameof(RankOptionDayManga));

        public static string RankOptionDayManga
        {
            get => rankOptionDayManga;
            set
            {
                rankOptionDayManga = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionWeekManga = GetResource(nameof(RankOptionWeekManga));

        public static string RankOptionWeekManga
        {
            get => rankOptionWeekManga;
            set
            {
                rankOptionWeekManga = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionWeekOriginal = GetResource(nameof(RankOptionWeekOriginal));

        public static string RankOptionWeekOriginal
        {
            get => rankOptionWeekOriginal;
            set
            {
                rankOptionWeekOriginal = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionWeekRookie = GetResource(nameof(RankOptionWeekRookie));

        public static string RankOptionWeekRookie
        {
            get => rankOptionWeekRookie;
            set
            {
                rankOptionWeekRookie = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionDayR18 = GetResource(nameof(RankOptionDayR18));

        public static string RankOptionDayR18
        {
            get => rankOptionDayR18;
            set
            {
                rankOptionDayR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionDayMaleR18 = GetResource(nameof(RankOptionDayMaleR18));

        public static string RankOptionDayMaleR18
        {
            get => rankOptionDayMaleR18;
            set
            {
                rankOptionDayMaleR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionDayFemaleR18 = GetResource(nameof(RankOptionDayFemaleR18));

        public static string RankOptionDayFemaleR18
        {
            get => rankOptionDayFemaleR18;
            set
            {
                rankOptionDayFemaleR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionWeekR18 = GetResource(nameof(RankOptionWeekR18));

        public static string RankOptionWeekR18
        {
            get => rankOptionWeekR18;
            set
            {
                rankOptionWeekR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankOptionWeekR18G = GetResource(nameof(RankOptionWeekR18G));

        public static string RankOptionWeekR18G
        {
            get => rankOptionWeekR18G;
            set
            {
                rankOptionWeekR18G = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankDateCannotBeNull = GetResource(nameof(RankDateCannotBeNull));

        public static string RankDateCannotBeNull
        {
            get => rankDateCannotBeNull;
            set
            {
                rankDateCannotBeNull = value;
                OnStaticPropertyChanged();
            }
        }

        public static string rankNeedR18On = GetResource(nameof(RankNeedR18On));

        public static string RankNeedR18On
        {
            get => rankNeedR18On;
            set
            {
                rankNeedR18On = value;
                OnStaticPropertyChanged();
            }
        }

        public static string cannotFindSpecifiedCertificate = GetResource(nameof(CannotFindSpecifiedCertificate));

        public static string CannotFindSpecifiedCertificate
        {
            get => cannotFindSpecifiedCertificate;
            set
            {
                cannotFindSpecifiedCertificate = value;
                OnStaticPropertyChanged();
            }
        }

        public static string gifIllustrationHint = GetResource(nameof(GifIllustrationHint));

        public static string GifIllustrationHint
        {
            get => gifIllustrationHint;
            set
            {
                gifIllustrationHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string mangaIllustrationHintFormat = GetResource(nameof(MangaIllustrationHintFormat));

        public static string MangaIllustrationHintFormat
        {
            get => mangaIllustrationHintFormat;
            set
            {
                mangaIllustrationHintFormat = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userIdHintFormat = GetResource(nameof(UserIdHintFormat));

        public static string UserIdHintFormat
        {
            get => userIdHintFormat;
            set
            {
                userIdHintFormat = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadSingleIllustration = GetResource(nameof(DownloadSingleIllustration));

        public static string DownloadSingleIllustration
        {
            get => downloadSingleIllustration;
            set
            {
                downloadSingleIllustration = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadAllInCurrentList = GetResource(nameof(DownloadAllInCurrentList));

        public static string DownloadAllInCurrentList
        {
            get => downloadAllInCurrentList;
            set
            {
                downloadAllInCurrentList = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadSpotlight = GetResource(nameof(DownloadSpotlight));

        public static string DownloadSpotlight
        {
            get => downloadSpotlight;
            set
            {
                downloadSpotlight = value;
                OnStaticPropertyChanged();
            }
        }

        public static string retractSidebar = GetResource(nameof(RetractSidebar));

        public static string RetractSidebar
        {
            get => retractSidebar;
            set
            {
                retractSidebar = value;
                OnStaticPropertyChanged();
            }
        }

        public static string restrictPolicy = GetResource(nameof(RestrictPolicy));

        public static string RestrictPolicy
        {
            get => restrictPolicy;
            set
            {
                restrictPolicy = value;
                OnStaticPropertyChanged();
            }
        }

        public static string _private = GetResource(nameof(Private));

        public static string Private
        {
            get => _private;
            set
            {
                _private = value;
                OnStaticPropertyChanged();
            }
        }

        public static string _public = GetResource(nameof(Public));

        public static string Public
        {
            get => _public;
            set
            {
                _public = value;
                OnStaticPropertyChanged();
            }
        }

        public static string homePage = GetResource(nameof(HomePage));

        public static string HomePage
        {
            get => homePage;
            set
            {
                homePage = value;
                OnStaticPropertyChanged();
            }
        }

        public static string myGallery = GetResource(nameof(MyGallery));

        public static string MyGallery
        {
            get => myGallery;
            set
            {
                myGallery = value;
                OnStaticPropertyChanged();
            }
        }

        public static string myFollowing = GetResource(nameof(MyFollowing));

        public static string MyFollowing
        {
            get => myFollowing;
            set
            {
                myFollowing = value;
                OnStaticPropertyChanged();
            }
        }

        public static string spotlight = GetResource(nameof(Spotlight));

        public static string Spotlight
        {
            get => spotlight;
            set
            {
                spotlight = value;
                OnStaticPropertyChanged();
            }
        }

        public static string recommend = GetResource(nameof(Recommend));

        public static string Recommend
        {
            get => recommend;
            set
            {
                recommend = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustRanking = GetResource(nameof(IllustRanking));

        public static string IllustRanking
        {
            get => illustRanking;
            set
            {
                illustRanking = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userTrend = GetResource(nameof(UserTrend));

        public static string UserTrend
        {
            get => userTrend;
            set
            {
                userTrend = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userUpdate = GetResource(nameof(UserUpdate));

        public static string UserUpdate
        {
            get => userUpdate;
            set
            {
                userUpdate = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchImageBySource = GetResource(nameof(SearchImageBySource));

        public static string SearchImageBySource
        {
            get => searchImageBySource;
            set
            {
                searchImageBySource = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadQueueAndHistory = GetResource(nameof(DownloadQueueAndHistory));

        public static string DownloadQueueAndHistory
        {
            get => downloadQueueAndHistory;
            set
            {
                downloadQueueAndHistory = value;
                OnStaticPropertyChanged();
            }
        }

        public static string setting = GetResource(nameof(Setting));

        public static string Setting
        {
            get => setting;
            set
            {
                setting = value;
                OnStaticPropertyChanged();
            }
        }

        public static string logout = GetResource(nameof(Logout));

        public static string Logout
        {
            get => logout;
            set
            {
                logout = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchHint = GetResource(nameof(SearchHint));

        public static string SearchHint
        {
            get => searchHint;
            set
            {
                searchHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string queryUser = GetResource(nameof(QueryUser));

        public static string QueryUser
        {
            get => queryUser;
            set
            {
                queryUser = value;
                OnStaticPropertyChanged();
            }
        }

        public static string querySingleUser = GetResource(nameof(QuerySingleUser));

        public static string QuerySingleUser
        {
            get => querySingleUser;
            set
            {
                querySingleUser = value;
                OnStaticPropertyChanged();
            }
        }

        public static string querySingleIllust = GetResource(nameof(QuerySingleIllust));

        public static string QuerySingleIllust
        {
            get => querySingleIllust;
            set
            {
                querySingleIllust = value;
                OnStaticPropertyChanged();
            }
        }

        public static string recommendIllustratorTurnPage = GetResource(nameof(RecommendIllustratorTurnPage));

        public static string RecommendIllustratorTurnPage
        {
            get => recommendIllustratorTurnPage;
            set
            {
                recommendIllustratorTurnPage = value;
                OnStaticPropertyChanged();
            }
        }

        public static string pixevalVersionFormat = GetResource(nameof(PixevalVersionFormat));

        public static string PixevalVersionFormat
        {
            get => pixevalVersionFormat;
            set
            {
                pixevalVersionFormat = value;
                OnStaticPropertyChanged();
            }
        }

        public static string aboutPixeval = GetResource(nameof(AboutPixeval));

        public static string AboutPixeval
        {
            get => aboutPixeval;
            set
            {
                aboutPixeval = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userBrowserFollowCountHint = GetResource(nameof(UserBrowserFollowCountHint));

        public static string UserBrowserFollowCountHint
        {
            get => userBrowserFollowCountHint;
            set
            {
                userBrowserFollowCountHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userBrowserFollow = GetResource(nameof(UserBrowserFollow));

        public static string UserBrowserFollow
        {
            get => userBrowserFollow;
            set
            {
                userBrowserFollow = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userBrowserUnFollow = GetResource(nameof(UserBrowserUnFollow));

        public static string UserBrowserUnFollow
        {
            get => userBrowserUnFollow;
            set
            {
                userBrowserUnFollow = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userBrowserPrivateFollow = GetResource(nameof(UserBrowserPrivateFollow));

        public static string UserBrowserPrivateFollow
        {
            get => userBrowserPrivateFollow;
            set
            {
                userBrowserPrivateFollow = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userBrowserIllustSelector = GetResource(nameof(UserBrowserIllustSelector));

        public static string UserBrowserIllustSelector
        {
            get => userBrowserIllustSelector;
            set
            {
                userBrowserIllustSelector = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userBrowserGallerySelector = GetResource(nameof(UserBrowserGallerySelector));

        public static string UserBrowserGallerySelector
        {
            get => userBrowserGallerySelector;
            set
            {
                userBrowserGallerySelector = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserIllustId = GetResource(nameof(IllustBrowserIllustId));

        public static string IllustBrowserIllustId
        {
            get => illustBrowserIllustId;
            set
            {
                illustBrowserIllustId = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserTotalViews = GetResource(nameof(IllustBrowserTotalViews));

        public static string IllustBrowserTotalViews
        {
            get => illustBrowserTotalViews;
            set
            {
                illustBrowserTotalViews = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserTotalBookmarks = GetResource(nameof(IllustBrowserTotalBookmarks));

        public static string IllustBrowserTotalBookmarks
        {
            get => illustBrowserTotalBookmarks;
            set
            {
                illustBrowserTotalBookmarks = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserResolution = GetResource(nameof(IllustBrowserResolution));

        public static string IllustBrowserResolution
        {
            get => illustBrowserResolution;
            set
            {
                illustBrowserResolution = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserUploadDate = GetResource(nameof(IllustBrowserUploadDate));

        public static string IllustBrowserUploadDate
        {
            get => illustBrowserUploadDate;
            set
            {
                illustBrowserUploadDate = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserIllustTag = GetResource(nameof(IllustBrowserIllustTag));

        public static string IllustBrowserIllustTag
        {
            get => illustBrowserIllustTag;
            set
            {
                illustBrowserIllustTag = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserSetWallpaper = GetResource(nameof(IllustBrowserSetWallpaper));

        public static string IllustBrowserSetWallpaper
        {
            get => illustBrowserSetWallpaper;
            set
            {
                illustBrowserSetWallpaper = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserShareLink = GetResource(nameof(IllustBrowserShareLink));

        public static string IllustBrowserShareLink
        {
            get => illustBrowserShareLink;
            set
            {
                illustBrowserShareLink = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserViewInBrowser = GetResource(nameof(IllustBrowserViewInBrowser));

        public static string IllustBrowserViewInBrowser
        {
            get => illustBrowserViewInBrowser;
            set
            {
                illustBrowserViewInBrowser = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserDownload = GetResource(nameof(IllustBrowserDownload));

        public static string IllustBrowserDownload
        {
            get => illustBrowserDownload;
            set
            {
                illustBrowserDownload = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserPrivateBookmark = GetResource(nameof(IllustBrowserPrivateBookmark));

        public static string IllustBrowserPrivateBookmark
        {
            get => illustBrowserPrivateBookmark;
            set
            {
                illustBrowserPrivateBookmark = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserBookmark = GetResource(nameof(IllustBrowserBookmark));

        public static string IllustBrowserBookmark
        {
            get => illustBrowserBookmark;
            set
            {
                illustBrowserBookmark = value;
                OnStaticPropertyChanged();
            }
        }

        public static string illustBrowserRemoveBookmark = GetResource(nameof(IllustBrowserRemoveBookmark));

        public static string IllustBrowserRemoveBookmark
        {
            get => illustBrowserRemoveBookmark;
            set
            {
                illustBrowserRemoveBookmark = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadQueueShowDownloadIllust = GetResource(nameof(DownloadQueueShowDownloadIllust));

        public static string DownloadQueueShowDownloadIllust
        {
            get => downloadQueueShowDownloadIllust;
            set
            {
                downloadQueueShowDownloadIllust = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadQueueRemoveFromDownloading = GetResource(nameof(DownloadQueueRemoveFromDownloading));

        public static string DownloadQueueRemoveFromDownloading
        {
            get => downloadQueueRemoveFromDownloading;
            set
            {
                downloadQueueRemoveFromDownloading = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadQueueDownloading = GetResource(nameof(DownloadQueueDownloading));

        public static string DownloadQueueDownloading
        {
            get => downloadQueueDownloading;
            set
            {
                downloadQueueDownloading = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadQueueEmptyNotifier = GetResource(nameof(DownloadQueueEmptyNotifier));

        public static string DownloadQueueEmptyNotifier
        {
            get => downloadQueueEmptyNotifier;
            set
            {
                downloadQueueEmptyNotifier = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadQueueDownloaded = GetResource(nameof(DownloadQueueDownloaded));

        public static string DownloadQueueDownloaded
        {
            get => downloadQueueDownloaded;
            set
            {
                downloadQueueDownloaded = value;
                OnStaticPropertyChanged();
            }
        }

        public static string emailOrPasswordIsWrong = GetResource(nameof(EmailOrPasswordIsWrong));

        public static string EmailOrPasswordIsWrong
        {
            get => emailOrPasswordIsWrong;
            set
            {
                emailOrPasswordIsWrong = value;
                OnStaticPropertyChanged();
            }
        }

        public static string copy = GetResource(nameof(Copy));

        public static string Copy
        {
            get => copy;
            set
            {
                copy = value;
                OnStaticPropertyChanged();
            }
        }

        public static string conditionBoxHint = GetResource(nameof(ConditionBoxHint));

        public static string ConditionBoxHint
        {
            get => conditionBoxHint;
            set
            {
                conditionBoxHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string pixevalSettings = GetResource(nameof(PixevalSettings));

        public static string PixevalSettings
        {
            get => pixevalSettings;
            set
            {
                pixevalSettings = value;
                OnStaticPropertyChanged();
            }
        }

        public static string sortByPopulation = GetResource(nameof(SortByPopulation));

        public static string SortByPopulation
        {
            get => sortByPopulation;
            set
            {
                sortByPopulation = value;
                OnStaticPropertyChanged();
            }
        }

        public static string turnOffR18 = GetResource(nameof(TurnOffR18));

        public static string TurnOffR18
        {
            get => turnOffR18;
            set
            {
                turnOffR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string turnOnIllustratorRecommend = GetResource(nameof(TurnOnIllustratorRecommend));

        public static string TurnOnIllustratorRecommend
        {
            get => turnOnIllustratorRecommend;
            set
            {
                turnOnIllustratorRecommend = value;
                OnStaticPropertyChanged();
            }
        }

        public static string turnOnDirectConnect = GetResource(nameof(TurnOnDirectConnect));

        public static string TurnOnDirectConnect
        {
            get => turnOnDirectConnect;
            set
            {
                turnOnDirectConnect = value;
                OnStaticPropertyChanged();
            }
        }

        public static string tagMatchOption = GetResource(nameof(TagMatchOption));

        public static string TagMatchOption
        {
            get => tagMatchOption;
            set
            {
                tagMatchOption = value;
                OnStaticPropertyChanged();
            }
        }

        public static string turnOnCache = GetResource(nameof(TurnOnCache));

        public static string TurnOnCache
        {
            get => turnOnCache;
            set
            {
                turnOnCache = value;
                OnStaticPropertyChanged();
            }
        }

        public static string memoryCachePolicy = GetResource(nameof(MemoryCachePolicy));

        public static string MemoryCachePolicy
        {
            get => memoryCachePolicy;
            set
            {
                memoryCachePolicy = value;
                OnStaticPropertyChanged();
            }
        }

        public static string fileCachePolicy = GetResource(nameof(FileCachePolicy));

        public static string FileCachePolicy
        {
            get => fileCachePolicy;
            set
            {
                fileCachePolicy = value;
                OnStaticPropertyChanged();
            }
        }

        public static string minBookmarkRequired = GetResource(nameof(MinBookmarkRequired));

        public static string MinBookmarkRequired
        {
            get => minBookmarkRequired;
            set
            {
                minBookmarkRequired = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchPageCountHint = GetResource(nameof(SearchPageCountHint));

        public static string SearchPageCountHint
        {
            get => searchPageCountHint;
            set
            {
                searchPageCountHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchPageStart = GetResource(nameof(SearchPageStart));

        public static string SearchPageStart
        {
            get => searchPageStart;
            set
            {
                searchPageStart = value;
                OnStaticPropertyChanged();
            }
        }

        public static string spotlightSearchPageStart = GetResource(nameof(SpotlightSearchPageStart));

        public static string SpotlightSearchPageStart
        {
            get => spotlightSearchPageStart;
            set
            {
                spotlightSearchPageStart = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadLocation = GetResource(nameof(DownloadLocation));

        public static string DownloadLocation
        {
            get => downloadLocation;
            set
            {
                downloadLocation = value;
                OnStaticPropertyChanged();
            }
        }

        public static string tagsToBeExclude = GetResource(nameof(TagsToBeExclude));

        public static string TagsToBeExclude
        {
            get => tagsToBeExclude;
            set
            {
                tagsToBeExclude = value;
                OnStaticPropertyChanged();
            }
        }

        public static string tagsToBeInclude = GetResource(nameof(TagsToBeInclude));

        public static string TagsToBeInclude
        {
            get => tagsToBeInclude;
            set
            {
                tagsToBeInclude = value;
                OnStaticPropertyChanged();
            }
        }

        public static string searchPerPageCountHint = GetResource(nameof(SearchPerPageCountHint));

        public static string SearchPerPageCountHint
        {
            get => searchPerPageCountHint;
            set
            {
                searchPerPageCountHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string turnOnWebR18 = GetResource(nameof(TurnOnWebR18));

        public static string TurnOnWebR18
        {
            get => turnOnWebR18;
            set
            {
                turnOnWebR18 = value;
                OnStaticPropertyChanged();
            }
        }

        public static string sauceNaoFileLocationHint = GetResource(nameof(SauceNaoFileLocationHint));

        public static string SauceNaoFileLocationHint
        {
            get => sauceNaoFileLocationHint;
            set
            {
                sauceNaoFileLocationHint = value;
                OnStaticPropertyChanged();
            }
        }

        public static string sauceNaoUploadAndSearch = GetResource(nameof(SauceNaoUploadAndSearch));

        public static string SauceNaoUploadAndSearch
        {
            get => sauceNaoUploadAndSearch;
            set
            {
                sauceNaoUploadAndSearch = value;
                OnStaticPropertyChanged();
            }
        }

        public static string signIn = GetResource(nameof(SignIn));

        public static string SignIn
        {
            get => signIn;
            set
            {
                signIn = value;
                OnStaticPropertyChanged();
            }
        }

        public static string signInAccount = GetResource(nameof(SignInAccount));

        public static string SignInAccount
        {
            get => signInAccount;
            set
            {
                signInAccount = value;
                OnStaticPropertyChanged();
            }
        }

        public static string signInPassword = GetResource(nameof(SignInPassword));

        public static string SignInPassword
        {
            get => signInPassword;
            set
            {
                signInPassword = value;
                OnStaticPropertyChanged();
            }
        }

        public static string signInButtonText = GetResource(nameof(SignInButtonText));

        public static string SignInButtonText
        {
            get => signInButtonText;
            set
            {
                signInButtonText = value;
                OnStaticPropertyChanged();
            }
        }

        public static string signInUpdatingSession = GetResource(nameof(SignInUpdatingSession));

        public static string SignInUpdatingSession
        {
            get => signInUpdatingSession;
            set
            {
                signInUpdatingSession = value;
                OnStaticPropertyChanged();
            }
        }

        public static string browsingHistoryCount = GetResource(nameof(BrowsingHistoryCount));

        public static string BrowsingHistoryCount
        {
            get => browsingHistoryCount;
            set
            {
                browsingHistoryCount = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadQueueBrowsingHistory = GetResource(nameof(DownloadQueueBrowsingHistory));

        public static string DownloadQueueBrowsingHistory
        {
            get => downloadQueueBrowsingHistory;
            set
            {
                downloadQueueBrowsingHistory = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadQueueHistoryListIsEmpty = GetResource(nameof(DownloadQueueHistoryListIsEmpty));

        public static string DownloadQueueHistoryListIsEmpty
        {
            get => downloadQueueHistoryListIsEmpty;
            set
            {
                downloadQueueHistoryListIsEmpty = value;
                OnStaticPropertyChanged();
            }
        }

        public static string pixevalUpdateAvailable = GetResource(nameof(PixevalUpdateAvailable));

        public static string PixevalUpdateAvailable
        {
            get => pixevalUpdateAvailable;
            set
            {
                pixevalUpdateAvailable = value;
                OnStaticPropertyChanged();
            }
        }

        public static string pixevalUpdateAvailableTitle = GetResource(nameof(PixevalUpdateAvailableTitle));

        public static string PixevalUpdateAvailableTitle
        {
            get => pixevalUpdateAvailableTitle;
            set
            {
                pixevalUpdateAvailableTitle = value;
                OnStaticPropertyChanged();
            }
        }

        public static string thisLoginSessionRequiresRecaptcha = GetResource(nameof(ThisLoginSessionRequiresRecaptcha));

        public static string ThisLoginSessionRequiresRecaptcha
        {
            get => thisLoginSessionRequiresRecaptcha;
            set
            {
                thisLoginSessionRequiresRecaptcha = value;
                OnStaticPropertyChanged();
            }
        }

        public static string supportMe = GetResource(nameof(SupportMe));

        public static string SupportMe
        {
            get => supportMe;
            set
            {
                supportMe = value;
                OnStaticPropertyChanged();
            }
        }

        public static string downloadTo = GetResource(nameof(DownloadTo));

        public static string DownloadTo
        {
            get => downloadTo;
            set
            {
                downloadTo = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userPreviewPopupFollow = GetResource(nameof(UserPreviewPopupFollow));

        public static string UserPreviewPopupFollow
        {
            get => userPreviewPopupFollow;
            set
            {
                userPreviewPopupFollow = value;
                OnStaticPropertyChanged();
            }
        }

        public static string userPreviewPopupUnFollow = GetResource(nameof(UserPreviewPopupUnFollow));

        public static string UserPreviewPopupUnFollow
        {
            get => userPreviewPopupUnFollow;
            set
            {
                userPreviewPopupUnFollow = value;
                OnStaticPropertyChanged();
            }
        }

        public static string selectCultureInfo = GetResource(nameof(SelectCultureInfo));

        public static string SelectCultureInfo
        {
            get => selectCultureInfo;
            set
            {
                selectCultureInfo = value;
                OnStaticPropertyChanged();
            }
        }

        public static string createNewFolderWhenDownloadFromUser = GetResource(nameof(CreateNewFolderWhenDownloadFromUser));

        public static string CreateNewFolderWhenDownloadFromUser
        {
            get => createNewFolderWhenDownloadFromUser;
            set
            {
                createNewFolderWhenDownloadFromUser = value;
                OnStaticPropertyChanged();
            }
        }
    }
}