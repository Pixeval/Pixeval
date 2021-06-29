using System.Threading.Tasks;
using Pixeval.CoreApi.Net.Request;
using Pixeval.CoreApi.Net.Response;
using Refit;

namespace Pixeval.CoreApi.Net.EndPoints
{
    internal interface IAppApiEndPoint
    {
        [Post("/v2/illust/bookmark/add")]
        Task AddBookmark([Body(BodySerializationMethod.UrlEncoded)] AddBookmarkRequest request);

        [Post("/v1/illust/bookmark/delete")]
        Task RemoveBookmark([Body(BodySerializationMethod.UrlEncoded)] RemoveBookmarkRequest request);
        
        [Get("/v1/illust/detail")]
        Task<PixivSingleIllustResponse> GetSingle([AliasAs("illust_id")] string id);

        [Get("/v1/user/detail")]
        Task<PixivSingleUserResponse> GetSingleUser(SingleUserRequest request);

        [Post("/v1/user/follow/add")]
        Task FollowUser([Body(BodySerializationMethod.UrlEncoded)] FollowUserRequest request);

        [Post("/v1/user/follow/delete")]
        Task RemoveFollowUser([Body(BodySerializationMethod.UrlEncoded)] RemoveFollowUserRequest request);

        [Get("/v1/trending-tags/illust")]
        Task<TrendingTagResponse> GetTrendingTags([AliasAs("filter")] string filter);
    }
}