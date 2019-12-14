using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using AngleSharp.Html.Parser;

namespace Pixeval.Core
{
    public class SpotlightContentExtractor
    {
        private readonly string spotlightId;

        public SpotlightContentExtractor(string spotlightId)
        {
            this.spotlightId = spotlightId;
        }

        public async Task<IEnumerable<string>> GetArticleWorks()
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync($"https://www.pixivision.net/en/a/{spotlightId}");

            var doc = await new HtmlParser().ParseDocumentAsync(html);

            return doc.QuerySelectorAll(".am__body .am__work")
                .Select(element => element.Children[1].Children[0].GetAttribute("href"))
                .Select(url => Regex.Match(url, "https://www.pixiv.net/artworks/(?<Id>\\d+)").Groups["Id"].Value);
        }
    }
}