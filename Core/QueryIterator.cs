using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Model.Web.Delegation;
using Pixeval.Data.Model.Web.Request;
using Pixeval.Objects.Exceptions;

namespace Pixeval.Core
{
    public class QueryIterator : IPixivIterator
    {
        private readonly string tag;

        private readonly int totalPages;

        private int currentIndex;

        public QueryIterator(string tag, int totalPages, int start = 1)
        {
            currentIndex = start;
            this.tag = tag;
            this.totalPages = totalPages;
        }

        public bool HasNext()
        {
            return currentIndex <= totalPages;
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            var works = await HttpClientFactory.PublicApiService.QueryWorks(new QueryWorksRequest {Tag = tag, Offset = currentIndex});

            if (currentIndex == 1 && !works.ToResponse.Any())
            {
                throw new QueryNotRespondingException();
            }

            foreach (var response in works.ToResponse)
            {
                yield return await PixivHelper.IllustrationInfo(response.Id.ToString());
            }

            currentIndex++;
        }
    }
}