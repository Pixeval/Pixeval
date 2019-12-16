using System.Collections.Generic;
using System.Linq;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Response;
using Pixeval.Objects;
using Pixeval.Objects.Exceptions;

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

        public bool HasNext()
        {
            if (context == null) return true;
            return !context.NextUrl.IsNullOrEmpty();
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            var httpClient = HttpClientFactory.PixivApi(ProtocolBase.AppApiBaseUrl);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer");

            var query = $"/v1/user/bookmarks/illust?user_id={uid}&restrict=public&filter=for_ios";

            var model = (await httpClient.GetStringAsync(context == null ? query : context.NextUrl)).FromJson<GalleryResponse>();

            if (context == null && model.Illusts.IsNullOrEmpty()) throw new QueryNotRespondingException();

            context = model;

            foreach (var contextIllust in context.Illusts.Where(contextIllust => contextIllust != null)) yield return await PixivHelper.IllustrationInfo(contextIllust.Id.ToString());
        }
    }
}