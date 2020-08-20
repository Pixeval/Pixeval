#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
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

#endregion

using System.Threading.Tasks;
using Pixeval.Wpf.Data.Web.Request;
using Pixeval.Wpf.Data.Web.Response;
using Refit;

namespace Pixeval.Wpf.Data.Web.Protocol
{
    [Headers("Authorization: Bearer")]
    public interface IAppApiProtocol
    {
        [Post("/v1/illust/bookmark/delete")]
        Task DeleteBookmark([Body(BodySerializationMethod.UrlEncoded)]
                            DeleteBookmarkRequest deleteBookmarkRequest);

        [Get("/v1/user/detail")]
        Task<UserInformationResponse> GetUserInformation(UserInformationRequest userInformationRequest);

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

        [Get("/v2/search/autocomplete")]
        Task<AutoCompletionResponse> GetAutoCompletion(AutoCompletionRequest autoCompletionRequest);

        [Get("/v1/illust/detail")]
        Task<SingleWorkResponse> GetSingle([AliasAs("illust_id")] string id);

        [Get("/v1/user/recommended?filter=for_android")]
        Task<RecommendIllustratorResponse> GetRecommendIllustrators(
            RecommendIllustratorRequest recommendIllustratorRequest);

        [Get("/v1/trending-tags/illust?filter=for_android")]
        Task<TrendingTagResponse> GetTrendingTags();
    }
}
