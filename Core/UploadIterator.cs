using System.Collections.Generic;
using System.Linq;
using Pixeval.Data.ViewModel;
using Pixeval.Data.Web.Delegation;
using Pixeval.Data.Web.Request;
using Pixeval.Objects.Exceptions;

namespace Pixeval.Core
{
    public class UploadIterator : IPixivIterator
    {
        private readonly int totalPages;

        private readonly string uid;

        private int currentIndex = 1;

        public UploadIterator(string uid, int totalPages)
        {
            this.uid = uid;
            this.totalPages = totalPages;
        }

        public bool HasNext()
        {
            return currentIndex <= totalPages;
        }

        public async IAsyncEnumerable<Illustration> MoveNextAsync()
        {
            var works = await HttpClientFactory.PublicApiService.GetUploads(uid, new UploadsRequest {Page = currentIndex++});
            if (currentIndex == 2 && !works.ToResponse.Any()) throw new QueryNotRespondingException();

            foreach (var response in works.ToResponse.Where(illustration => illustration != null)) yield return await PixivHelper.IllustrationInfo(response.Id.ToString());
        }
    }
}