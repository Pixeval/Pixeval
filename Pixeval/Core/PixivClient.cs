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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AngleSharp.Html.Parser;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Protocol;
using Pixeval.Data.Web.Request;
using Pixeval.Data.Web.Response;
using Pixeval.Objects.Exceptions;
using Pixeval.Objects.Exceptions.Logger;
using Pixeval.Objects.Generic;
using Pixeval.Objects.I18n;
using Pixeval.Objects.Primitive;
using Pixeval.Persisting;
using Refit;

namespace Pixeval.Core
{
    public class PixivClient
    {
        public static async Task Login(string account, string password)
        {
            await Authentication.AuthenticateInternal(account, password);
        }

        public static async Task Login(string refreshToken)
        {
            await Authentication.AuthenticateInternal(refreshToken);
        }

        public static async Task Refresh()
        {
            await Authentication.RefreshInternal();
        }

        public static async Task<Illustration> IllustrationInfo(string id)
        {
            SingleWorkResponse.Illust response;
            try
            {
                response = (await HttpClientFactory.AppApiService().GetSingle(id)).IllustInfo;
            }
            catch (ApiException e)
            {
                ExceptionDumper.WriteException(e);
                return null;
            }
            catch (Exception)
            {
                return null;
            }

            var illust = new Illustration
            {
                Bookmark = (int)response.TotalBookmarks,
                Id = response.Id.ToString(),
                IsLiked = response.IsBookmarked,
                IsManga = response.PageCount != 1,
                IsUgoira = response.Type == "ugoira",
                Origin = response.ImageUrls.Original ?? response.MetaSinglePage.OriginalImageUrl,
                Large = response.ImageUrls.Large,
                Tags = response.Tags.Select(t => new Tag { Name = t.Name, TranslatedName = t.TranslatedName }),
                Thumbnail = response.ImageUrls.Medium,
                Title = response.Title,
                UserName = response.User.Name,
                UserId = response.User.Id.ToString(),
                ViewCount = (int)response.TotalView,
                Comments = (int)response.TotalComments,
                Resolution = $"{response.Width}x{response.Height}",
                PublishDate = response.CreateDate
            };

            if (illust.IsManga && response.MetaPages != null)
            {
                illust.MangaMetadata = response.MetaPages.Select(p =>
                {
                    var page = (Illustration)illust.Clone();
                    page.Thumbnail = p.ImageUrls.Medium;
                    page.Origin = p.ImageUrls.Original;
                    page.Large = p.ImageUrls.Large;
                    return page;
                }).ToArray();
            }

            return illust;
        }

        public static async void Enumerate<T>(IPixivAsyncEnumerable<T> pixivIterator, IList<T> container, int limit = -1)
        {
            EnumeratingSchedule.StartNewInstance(pixivIterator);
            var enumerator = EnumeratingSchedule.GetCurrentEnumerator<T>();

            await foreach (var illust in enumerator)
            {
                if (enumerator.IsCancellationRequested() || limit != -1 && pixivIterator.RequestedPages > limit)
                {
                    break;
                }
                if (pixivIterator.Verify(illust, container))
                {
                    pixivIterator.InsertionPolicy(illust, container);
                }
            }
        }

        public static void RecordTimeline(ITimelineService service, BrowsingHistory browsingHistory)
        {
            if (service.VerifyRationality(browsingHistory))
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Loaded, (Action) (() => service.Insert(browsingHistory)));
            }
        }

        public static void RecordTimelineInternal(BrowsingHistory browsingHistory)
        {
            RecordTimeline(BrowsingHistoryAccessor.GlobalLifeTimeScope, browsingHistory);
            if (CheckWindowsVersion())
            {
                RecordTimeline(WindowsUserActivityManager.GlobalLifeTimeScope, browsingHistory);
            }

            static bool CheckWindowsVersion()
            {
                return Environment.OSVersion.Version >= new Version(10, 0, 17134); /* Windows 10 April 2018 Update */
            }
        }

        public static bool VerifyIllust(ISet<string> excludeTag, ISet<string> includeTag, int minBookmark, Illustration illustration)
        {
            if (illustration == null)
            {
                return false;
            }
            bool excludeMatch = true, includeMatch = true;
            if (!excludeTag.IsNullOrEmpty())
            {
                excludeMatch = excludeTag.All(x => x.IsNullOrEmpty() || illustration.Tags.All(i => !i.Name.EqualsIgnoreCase(x)));
            }

            if (!includeTag.IsNullOrEmpty())
            {
                includeMatch = includeTag.All(x => x.IsNullOrEmpty() || illustration.Tags.Any(i => i.Name.EqualsIgnoreCase(x)));
            }

            var minBookmarkMatch = illustration.Bookmark > minBookmark;
            return excludeMatch && includeMatch && minBookmarkMatch;
        }

        public static async void PostFavoriteAsync(Illustration illustration, RestrictPolicy restrictPolicy)
        {
            illustration.IsLiked = true;
            await HttpClientFactory.AppApiService().AddBookmark(new AddBookmarkRequest { Id = illustration.Id, Restrict = restrictPolicy == RestrictPolicy.Public ? "public" : "private" });
        }

        public static async void RemoveFavoriteAsync(Illustration illustration)
        {
            illustration.IsLiked = false;
            await HttpClientFactory.AppApiService().DeleteBookmark(new DeleteBookmarkRequest { IllustId = illustration.Id });
        }

        public static async Task<IEnumerable<string>> GetArticleWorks(string spotlightId)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync($"https://www.pixivision.net/en/a/{spotlightId}");

            var doc = await new HtmlParser().ParseDocumentAsync(html);

            return doc.QuerySelectorAll(".am__body .am__work").Select(element => element.Children[1].Children[0].GetAttribute("href")).Select(url => Regex.Match(url, "https://www.pixiv.net/artworks/(?<Id>\\d+)").Groups["Id"].Value);
        }

        public static async Task FollowArtist(User user, RestrictPolicy policy)
        {
            user.IsFollowed = true;
            await HttpClientFactory.AppApiService().FollowArtist(new FollowArtistRequest { Id = user.Id, Restrict = policy == RestrictPolicy.Private ? "private" : "public" });
        }

        public static async Task UnFollowArtist(User user)
        {
            user.IsFollowed = false;
            await HttpClientFactory.AppApiService().UnFollowArtist(new UnFollowArtistRequest { UserId = user.Id });
        }

        public static async Task<List<TrendingTag>> GetTrendingTags()
        {
            var result = await HttpClientFactory.AppApiService().GetTrendingTags();
            var list = new List<TrendingTag>();
            if (result is { } res)
            {
                list.AddRange(res.TrendTags.Select(tag => new TrendingTag { Tag = tag.TagStr, TranslatedName = tag.TranslatedName, Thumbnail = tag.Illust.ImageUrls.SquareMedium }));
            }
            return list;
        }

        [Obsolete("reserved for Web API, this API is currently unstable and subject to change in the future")]
        public static async ValueTask<bool> ToggleWebApiR18State(bool isR18On)
        {
            try
            {
                var html = await HttpClientFactory.WebApiHttpClient().GetStringAsync("https://www.pixiv.net/setting_user.php");
                var doc = await new HtmlParser().ParseDocumentAsync(html);

                var tt = doc.QuerySelectorAll(".settingContent form input")[1].GetAttribute("value");
                await HttpClientFactory.WebApiService().ToggleR18State(new ToggleR18StateRequest { R18 = isR18On ? "show" : "hide", R18G = isR18On ? "2" : "1", Tt = tt });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static class Authentication
        {
            private const string ClientHash = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";

            private static string UtcTimeNow => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss+00:00");

            public static async Task AuthenticateInternal(string name, string pwd)
            {
                var time = UtcTimeNow;
                var hash = (time + ClientHash).Hash<MD5CryptoServiceProvider>();

                try
                {
                    var token = await RestService.For<ITokenProtocol>(HttpClientFactory.PixivAuthApi().Apply(h => h.Timeout = TimeSpan.FromSeconds(10))).GetTokenByPassword(new PasswordTokenRequest { Name = name, Password = pwd }, time, hash);
                    Session.Current = Session.Parse(pwd, token);
                }
                catch (TaskCanceledException)
                {
                    throw new AuthenticateFailedException(AkaI18N.AppApiAuthenticateTimeout);
                }
            }

            public static async Task AuthenticateInternal(string refreshToken)
            {
                try
                {
                    var token = await RestService.For<ITokenProtocol>(HttpClientFactory.PixivAuthApi().Apply(h => h.Timeout = TimeSpan.FromSeconds(10))).RefreshToken(new RefreshTokenRequest { RefreshToken = refreshToken });
                    Session.Current = Session.Parse(Session.Current.Password, token);
                }
                catch (TaskCanceledException)
                {
                    throw new AuthenticateFailedException(AkaI18N.AppApiAuthenticateTimeout);
                }
            }

            public static async Task RefreshInternal()
            {
                if (Session.Current.RefreshToken is { } token)
                {
                    await AuthenticateInternal(token);
                }
                else
                {
                    if (!EnsureConfig())
                    {
                        throw new AuthenticateFailedException("account or password is empty"); // TODO localization
                    }
                    await AuthenticateInternal(Session.Current.Account, Session.Current.Password);
                }
            }

            private static bool EnsureConfig()
            {
                return !Session.Current.Account.IsNullOrEmpty() && Session.Current.Password.IsNullOrEmpty();
            }
        }
    }
}