using System.Threading.Tasks;
using Pzxlane.Data.Model.Web.Request;
using Pzxlane.Data.Model.Web.Response;
using Refit;

namespace Pzxlane.Data.Model.Web.Protocol
{
    [Headers("Authorization: Bearer")]
    public interface IPublicApiProtocol
    {
        [Get("/search/works.json")]
        Task<QueryResponse> QueryWorks(QueryWorksRequest queryWorksRequest);

        [Get("/users/{uid}/works.json")]
        Task<UploadResponse> GetUploads(string uid, UploadsRequest uploadResponse);

        [Get("/works/{uid}.json")]
        Task<IllustResponse> GetSingle(string uid, [AliasAs("image_sizes")] string imageSizes = "px_128x128,small,medium,large,px_480mw", [AliasAs("include_stats")] string includeStat = "true");
    }
}