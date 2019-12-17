// Pixeval
// Copyright (C) 2019 Dylech30th <decem0730@gmail.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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

        [Get("/v1/search/user")]
        Task<UserNavResponse> GetUserNav(string word, int offset = 0, string filter = "for_android");
    }
}