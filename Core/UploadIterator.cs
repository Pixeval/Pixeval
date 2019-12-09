using System.Collections.Generic;
using Pixeval.Data.Model.ViewModel;
using Pixeval.Data.Model.Web.Delegation;
using Pixeval.Data.Model.Web.Request;

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

            foreach (var response in works.ToResponse)
            {
                yield return await PixivHelper.IllustrationInfo(response.Id.ToString());
            }
        }
    }
}