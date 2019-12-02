using System.Collections.Generic;
using System.Threading.Tasks;
using Pzxlane.Api.Supplier;
using Pzxlane.Data.Model.Web.Delegation;
using Pzxlane.Data.Model.Web.Request;
using Pzxlane.Data.Model.Web.Response;

namespace Pzxlane.Api.Impl
{
    public sealed class Upload : IIterableContentSupplier<UploadResponse.Response, UploadResponse>
    {
        private readonly string userId;

        public Upload(string userId)
        {
            this.userId = userId;
        }

        public string GetIllustId(UploadResponse.Response entity)
        {
            return entity.Id.ToString();
        }

        public async Task<IEnumerable<UploadResponse.Response>> GetIllusts(object param)
        {
            Context = await HttpClientFactory.PublicApiService.GetUploads(new UploadsRequest {Uid = userId, Page = (int) param});
            return Context.ToResponse;
        }

        public UploadResponse Context { get; private set; }

        public string Status => Context.Status;

        public int Start => 0;

        public int End => 0;
    }
}