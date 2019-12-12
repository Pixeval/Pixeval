using System.Collections.Generic;
using System.Windows;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Pixeval.Persisting;

namespace Pixeval.Core
{
    public class RankingIterator : IPixivIterator
    {
        private RankingResponse context;

        private int limit;

        public bool HasNext()
        {
            if (context == null) return true;

            if (limit++ >= 5) return false;

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