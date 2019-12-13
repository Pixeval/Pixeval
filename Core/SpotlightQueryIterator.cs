using System.Collections.Generic;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Web.Delegation;

namespace Pixeval.Core
{
    public class SpotlightQueryIterator
    {
        private readonly int queryPages;
        private readonly int start;

        public SpotlightQueryIterator(int start, int queryPages)
        {
            this.start = start < 1 ? 1 : start;
            this.queryPages = queryPages;
        }

        public async IAsyncEnumerable<SpotlightArticle> GetSpotlight()
        {
            for (var i = start; i < start + queryPages - 1; i++)
            {
                var spotlight = await HttpClientFactory.AppApiService.GetSpotlights(i * 10);
                foreach (var spotlightSpotlightArticle in spotlight.SpotlightArticles) yield return spotlightSpotlightArticle;
            }
        }
    }
}