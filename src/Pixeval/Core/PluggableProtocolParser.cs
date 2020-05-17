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

using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Objects.Generic;
using Pixeval.UI;
using Refit;

namespace Pixeval.Core
{
    public class PluggableProtocolParser
    {
        private const string IllustRegex = "pixeval://(www\\.)?pixiv\\.net/artworks/(?<id>\\d+)";
        private const string UserRegex = "pixeval://(www\\.)?pixiv\\.net/users/(?<id>\\d+)";
        private const string SpotlightRegex = "pixeval://(www\\.)?pixivision\\.net/\\w{2}/a/(?<id>\\d+)";

        public static async Task Parse(string url)
        {
            Match match;
            if ((match = Regex.Match(url, IllustRegex)).Success)
            {
                if (MainWindow.Instance.IllustBrowserDialogHost.IsOpen)
                    MainWindow.Instance.IllustBrowserDialogHost.CurrentSession.Close();
                MainWindow.Instance.OpenIllustBrowser(await PixivHelper.IllustrationInfo(match.Groups["id"].Value));
            }
            else if ((match = Regex.Match(url, UserRegex)).Success)
            {
                MainWindow.Instance.SetUserBrowserContext(new User {Id = match.Groups["id"].Value});
                MainWindow.Instance.OpenUserBrowser();
            }
            else if ((match = Regex.Match(url, SpotlightRegex)).Success)
            {
                var articleId = match.Groups["id"].Value;
                string title;

                // if the current culture is set to en-XX, then we will simply analyze the html and get the title
                if (CultureInfo.CurrentCulture.Name.ToLower().StartsWith("en"))
                    title = await TryGetSpotlightEnTitle(articleId);
                else // otherwise, we will try to access the spotlight web API, and analyze html if failed
                    try
                    {
                        title = (await HttpClientFactory.WebApiService().GetSpotlightArticles(articleId)).BodyList[0]
                            .Title;
                    }
                    catch (ApiException e)
                    {
                        if (e.StatusCode == HttpStatusCode.NotFound)
                            title = await TryGetSpotlightEnTitle(articleId);
                        else throw;
                    }

                var tasks = await Tasks<string, Illustration>.Of(await PixivClient.Instance.GetArticleWorks(articleId))
                    .Mapping(PixivHelper.IllustrationInfo)
                    .Construct()
                    .WhenAll();
                var result = tasks.Peek(i =>
                {
                    i.IsManga = true;
                    i.FromSpotlight = true;
                    i.SpotlightTitle = title;
                }).ToArray();

                MainWindow.Instance.OpenIllustBrowser(result[0].Apply(r => r.MangaMetadata = result.ToArray()));

                static async Task<string> TryGetSpotlightEnTitle(string id)
                {
                    var httpClient = new HttpClient();
                    var html = await httpClient.GetStringAsync($"https://www.pixivision.net/en/a/{id}");
                    var doc = await new HtmlParser().ParseDocumentAsync(html);
                    return doc.QuerySelectorAll(".am__title")[0].InnerHtml;
                }
            }
        }
    }
}