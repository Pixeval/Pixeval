#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/PixivSingleUserResponse.cs
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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response;

// ReSharper disable UnusedAutoPropertyAccessor.Global
public class PixivSingleUserResponse
{
    [JsonPropertyName("user")]
    public required UserInfo UserEntity { get; set; }

    [JsonPropertyName("profile")]
    public required Profile UserProfile { get; set; }

    [JsonPropertyName("profile_publicity")]
    public required ProfilePublicity UserProfilePublicity { get; set; }

    [JsonPropertyName("workspace")]
    public required Workspace UserWorkspace { get; set; }

    public class Profile
    {
        [JsonPropertyName("webpage")]
        public required string Webpage { get; set; }

        [JsonPropertyName("gender")]
        public required string Gender { get; set; }

        [JsonPropertyName("birth")]
        public required string Birth { get; set; }

        [JsonPropertyName("birth_day")]
        public required string BirthDay { get; set; }

        [JsonPropertyName("birth_year")]
        public required long BirthYear { get; set; }

        [JsonPropertyName("region")]
        public required string Region { get; set; }

        [JsonPropertyName("address_id")]
        public required long AddressId { get; set; }

        [JsonPropertyName("country_code")]
        public required string CountryCode { get; set; }

        [JsonPropertyName("job")]
        public required string Job { get; set; }

        [JsonPropertyName("job_id")]
        public required long JobId { get; set; }

        [JsonPropertyName("total_follow_users")]
        public required long TotalFollowUsers { get; set; }

        [JsonPropertyName("total_mypixiv_users")]
        public required long TotalMyPixivUsers { get; set; }

        [JsonPropertyName("total_illusts")]
        public required long TotalIllusts { get; set; }

        [JsonPropertyName("total_manga")]
        public required long TotalManga { get; set; }

        [JsonPropertyName("total_novels")]
        public required long TotalNovels { get; set; }

        [JsonPropertyName("total_illust_bookmarks_public")]
        public required long TotalIllustBookmarksPublic { get; set; }

        [JsonPropertyName("total_illust_series")]
        public required long TotalIllustSeries { get; set; }

        [JsonPropertyName("total_novel_series")]
        public required long TotalNovelSeries { get; set; }

        [JsonPropertyName("background_image_url")]
        public required string BackgroundImageUrl { get; set; }

        [JsonPropertyName("twitter_account")]
        public required string TwitterAccount { get; set; }

        [JsonPropertyName("twitter_url")]
        public required string TwitterUrl { get; set; }

        [JsonPropertyName("is_premium")]
        public required bool IsPremium { get; set; }

        [JsonPropertyName("is_using_custom_profile_image")]
        public required bool IsUsingCustomProfileImage { get; set; }
    }

    public class ProfilePublicity
    {
        [JsonPropertyName("gender")]
        public required string Gender { get; set; }

        [JsonPropertyName("region")]
        public required string Region { get; set; }

        [JsonPropertyName("birth_day")]
        public required string BirthDay { get; set; }

        [JsonPropertyName("birth_year")]
        public required string BirthYear { get; set; }

        [JsonPropertyName("job")]
        public required string Job { get; set; }

        [JsonPropertyName("pawoo")]
        public required bool Pawoo { get; set; }
    }

    public class ProfileImageUrls
    {
        [JsonPropertyName("medium")]
        public required string Medium { get; set; }
    }

    public class Workspace
    {
        [JsonPropertyName("pc")]
        public required string Pc { get; set; }

        [JsonPropertyName("monitor")]
        public required string Monitor { get; set; }

        [JsonPropertyName("tool")]
        public required string Tool { get; set; }

        [JsonPropertyName("scanner")]
        public required string Scanner { get; set; }

        [JsonPropertyName("tablet")]
        public required string Tablet { get; set; }

        [JsonPropertyName("mouse")]
        public required string Mouse { get; set; }

        [JsonPropertyName("printer")]
        public required string Printer { get; set; }

        [JsonPropertyName("desktop")]
        public required string Desktop { get; set; }

        [JsonPropertyName("music")]
        public required string Music { get; set; }

        [JsonPropertyName("desk")]
        public required string Desk { get; set; }

        [JsonPropertyName("chair")]
        public required string Chair { get; set; }

        [JsonPropertyName("comment")]
        public required string Comment { get; set; }
    }
}