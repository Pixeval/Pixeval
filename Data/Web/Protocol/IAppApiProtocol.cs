using System.Threading.Tasks;
using Pixeval.Data.Web.Request;
using Pixeval.Data.Web.Response;
using Refit;

namespace Pixeval.Data.Web.Protocol
{
    [Headers("Authorization: Bearer")]
    public interface IAppApiProtocol
    {
        [Get("/v1/illust/recommended")]
        Task<RankingResponse> GetRanking(RankingRequest rankingRequest);

        [Post("/v1/illust/bookmark/delete")]
        Task DeleteBookmark([Body(BodySerializationMethod.UrlEncoded)]
            DeleteBookmarkRequest deleteBookmarkRequest);

        [Get("/v1/user/detail")]
        Task<UserInformationResponse> GetUserInformation(UserInformationRequest userInformationRequest);

        [Get("/v1/user/bookmarks/illust")]
        Task<GalleryResponse> GetGallery(GalleryRequest favoriteWorkRequest);

        [Headers("Accept-Language: zh-cn")]
        [Get("/v1/spotlight/articles?category=all")]
        Task<SpotlightResponse> GetSpotlights(int offset);

        [Get("/v1/user/following")]
        Task<FollowingResponse> GetFollowing(FollowingRequest followingRequest);

        [Post("/v1/user/follow/add")]
        Task FollowArtist([Body(BodySerializationMethod.UrlEncoded)]
            FollowArtistRequest followArtistRequest);

        [Post("/v1/user/follow/delete")]
        Task UnFollowArtist([Body(BodySerializationMethod.UrlEncoded)]
            UnFollowArtistRequest unFollowArtistRequest);

        [Get("/v1/ugoira/metadata")]
        Task<UgoiraMetadataResponse> GetUgoiraMetadata([AliasAs("illust_id")] string id);

        [Post("/v2/illust/bookmark/add")]
        Task AddBookmark([Body(BodySerializationMethod.UrlEncoded)]
            AddBookmarkRequest addBookmarkRequest);

        [Get("/v1/search/user?filter=for_android")]
        Task<UserNavResponse> GetUserNav(string word, int offset = 0);
    }
}