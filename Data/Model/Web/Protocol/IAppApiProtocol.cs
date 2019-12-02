using System.Threading.Tasks;
using Pzxlane.Data.Model.Web.Request;
using Pzxlane.Data.Model.Web.Response;
using Refit;

namespace Pzxlane.Data.Model.Web.Protocol
{
    [Headers("Authorization: Bearer")]
    public interface IAppApiProtocol
    {
        [Get("/v1/illust/recommended")]
        Task<RankingResponse> GetRanking(RankingRequest rankingRequest);

        [Post("/v1/illust/bookmark/delete")]
        Task DeleteBookmark([Body(BodySerializationMethod.UrlEncoded)][AliasAs("illust_id")] string illustId);

        [Get("/v1/user/detail")]
        Task<UserInformationResponse> GetUserInformation(UserInformationRequest userInformationRequest);

        [Get("/v1/user/bookmarks/illust")]
        Task<FavoriteWorksResponse> GetFavoriteWorks(FavoriteWorkRequest favoriteWorkRequest);

        [Get("/v1/spotlight/articles?category=all")]
        Task<SpotlightResponse> GetSpotlights(int offset);

        [Get("/v1/user/following")]
        Task<FollowingResponse> GetFollowing(FollowingRequest followingRequest);

        [Post("/v1/user/follow/add")]
        Task FollowArtist([Body(BodySerializationMethod.UrlEncoded)] FollowArtistRequest followArtistRequest);

        [Post("/v1/user/follow/delete")]
        Task UnFollowArtist([Body(BodySerializationMethod.UrlEncoded)] [AliasAs("user_id")] string id);

        [Get("/v1/ugoira/metadata")]
        Task<UgoiraMetadataResponse> GetUgoiraMetadata([AliasAs("illust_id")] string id);

        [Post("/v2/illust/bookmark/add")]
        Task AddBookmark([Body(BodySerializationMethod.UrlEncoded)] AddBookmarkRequest addBookmarkRequest);

        [Get("/v1/search/user?filter=for_android")]
        Task<UserNavResponse> GetUserNav(string word, int offset = 0);
    }
}