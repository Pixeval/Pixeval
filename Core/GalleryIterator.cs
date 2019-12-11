using System;
using System.Collections.Generic;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Pixeval.Persisting;

namespace Pixeval.Core
{
    public class GalleryIterator : IPixivIterator
    {
        private readonly string uid;

        private GalleryResponse context;

        public GalleryIterator(string uid)
        {
            this.uid = uid;
        }

        public event Action Finalize;

        public bool HasNext()
        {
            if (context == null) return true;
            return !context.NextUrl.IsNullOrEmpty();
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            var httpClient = HttpClientFactory.PixivApi(ProtocolBase.PublicApiBaseUrl);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {Identity.Global.AccessToken}");

            var query = $"/v1/user/bookmarks/illust?user_id={uid}&restrict=public&filter=for_ios";
            context = (await httpClient.GetStringAsync(context == null ? query : context.NextUrl)).FromJson<GalleryResponse>();

            foreach (var contextIllust in context.Illusts)
            {
                yield return await PixivHelper.IllustrationInfo(contextIllust.Id.ToString());
            }
        }
    }
}