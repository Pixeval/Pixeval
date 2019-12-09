using System.Collections.Generic;
using Pixeval.Caching.Persisting;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Model.Web;
using Pixeval.Data.Model.Web.Delegation;
using Pixeval.Data.Model.Web.Response;
using Pixeval.Objects;

namespace Pixeval.Core
{
    public class RankingIterator : IPixivIterator
    {
        private RankingResponse context;

        public bool HasNext()
        {
            if (context == null) return true;
            return !context.NextUrl.IsNullOrEmpty();
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            var httpClient = HttpClientFactory.PixivApi(ProtocolBase.AppApiBaseUrl);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {Identity.Global.AccessToken}");

            const string query = "/v1/illust/recommended";
            context = (await httpClient.GetStringAsync(context == null ? query : context.NextUrl)).FromJson<RankingResponse>();

            foreach (var contextIllust in context.Illusts)
            {
                yield return await PixivHelper.IllustrationInfo(contextIllust.Id.ToString());
            }
        }
    }
}