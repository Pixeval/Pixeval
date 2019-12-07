using System.Collections.Generic;
using Pzxlane.Caching.Persisting;
using Pzxlane.Data.Model.ViewModel;
using Pzxlane.Data.Model.Web;
using Pzxlane.Data.Model.Web.Delegation;
using Pzxlane.Data.Model.Web.Response;
using Pzxlane.Objects;

namespace Pzxlane.Core
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