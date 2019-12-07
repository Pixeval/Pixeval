using System.Collections.Generic;
using Pzxlane.Data.Model.ViewModel;
using Pzxlane.Data.Model.Web.Delegation;
using Pzxlane.Data.Model.Web.Request;

namespace Pzxlane.Core
{
    public class QueryIterator : IPixivIterator
    {
        private readonly string tag;

        private readonly int totalPages;

        private int currentIndex = 1;

        public QueryIterator(string tag, int totalPages)
        {
            this.tag = tag;
            this.totalPages = totalPages;
        }

        public bool HasNext()
        {
            return currentIndex <= totalPages;
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            var works = await HttpClientFactory.PublicApiService.QueryWorks(new QueryWorksRequest {Tag = tag, Offset = currentIndex++});

            foreach (var response in works.ToResponse)
            {
                yield return await PixivHelper.IllustrationInfo(response.Id.ToString());
            }
        }
    }
}