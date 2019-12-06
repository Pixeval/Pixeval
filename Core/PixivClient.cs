using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Documents;
using Pzxlane.Caching.Persisting;
using Pzxlane.Data.Model.ViewModel;
using Pzxlane.Data.Model.Web;
using Pzxlane.Data.Model.Web.Delegation;
using Pzxlane.Data.Model.Web.Request;
using Pzxlane.Data.Model.Web.Response;
using Pzxlane.Objects;
using Page = System.Collections.Generic.IEnumerable<Pzxlane.Data.Model.ViewModel.Illustration>;

namespace Pzxlane.Core
{
    public sealed class PixivClient
    {
        private static volatile PixivClient _instance;
        
        private static readonly object Locker = new object();

        public static PixivClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance == null)
                            _instance = new PixivClient();
                    }
                }

                return _instance;
            }
        }

        public async IAsyncEnumerable<Task<Illustration>> Recommend()
        {
            foreach (var illust in (await HttpClientFactory.AppApiService.GetRanking(new RankingRequest())).Illusts)
            {
                yield return PixivHelper.IllustrationInfo(illust.Id.ToString());
            }
        }

        public async IAsyncEnumerable<Task<Illustration>> Gallery(string uid)
        {
            var httpClient = HttpClientFactory.PixivApi(ProtocolBase.AppApiBaseUrl);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {Identity.Global.AccessToken}");

            var url = "/v1/user/bookmarks/illust";

            while (!url.IsNullOrEmpty())
            {
                var model = (await httpClient.GetStringAsync(url)).FromJson<GalleryResponse>();

                foreach (var responseIllust in model.Illusts)
                {
                    yield return PixivHelper.IllustrationInfo(responseIllust.Id.ToString());
                }
                url = model.NextUrl;
            }
        }

        public async IAsyncEnumerable<Task<Illustration>> Query(string tag)
        {
            var illustPages = await PixivHelper.GetQueryPagesCount(tag);

            foreach (var i in Enumerable.Range(1, illustPages))
            {
                var responses = await HttpClientFactory.PublicApiService.QueryWorks(new QueryWorksRequest { Tag = tag, Offset = i });

                foreach (var response in responses.ToResponse)
                {
                    yield return PixivHelper.IllustrationInfo(response.Id.ToString());
                }
            }
        }

        public async IAsyncEnumerable<Task<Illustration>> Upload(string uid)
        {
            var illustPages = await PixivHelper.GetUploadPagesCount(uid);

            foreach (var i in Enumerable.Range(1, illustPages))
            {
                var responses = await HttpClientFactory.PublicApiService.GetUploads(uid, new UploadsRequest{ Page = i });

                foreach (var response in responses.ToResponse)
                {
                    yield return PixivHelper.IllustrationInfo(response.Id.ToString());
                }
            }
        }
    }
}