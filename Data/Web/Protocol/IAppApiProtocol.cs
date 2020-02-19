// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
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
        [Post("/v1/illust/bookmark/delete")]
        Task DeleteBookmark([Body(BodySerializationMethod.UrlEncoded)]
            DeleteBookmarkRequest deleteBookmarkRequest);

        [Get("/v1/user/detail")]
        Task<UserInformationResponse> GetUserInformation(UserInformationRequest userInformationRequest);

        [Headers("Accept-Language: zh-cn")]
        [Get("/v1/spotlight/articles?category=all")]
        Task<SpotlightResponse> GetSpotlights(int offset);

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

        [Headers("Accept-Language: en-us")]
        [Get("/v2/search/autocomplete")]
        Task<AutoCompletionResponse> GetAutoCompletion(AutoCompletionRequest autoCompletionRequest);

        [Headers("Accept-Language: zn-cn")]
        [Get("/v1/illust/detail")]
        Task<SingleWorkResponse> GetSingle([AliasAs("illust_id")] string id);

        [Headers("Accept-Language: zn-cn")]
        [Get("/v1/user/recommended?filter=for_android")]
        Task<RecommendIllustratorResponse> GetRecommendIllustrators(RecommendIllustratorRequest recommendIllustratorRequest);
    }
}