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
using System;
using Newtonsoft.Json;

namespace Pixeval.Data.Web.Response
{
    public class UserInformationResponse
    {
        [JsonProperty("user")]
        public User UserEntity { get; set; }

        [JsonProperty("profile")]
        public Profile UserProfile { get; set; }

        [JsonProperty("profile_publicity")]
        public ProfilePublicity UserProfilePublicity { get; set; }

        [JsonProperty("workspace")]
        public Workspace UserWorkspace { get; set; }

        public class Profile
        {
            [JsonProperty("webpage")]
            public Uri Webpage { get; set; }

            [JsonProperty("gender")]
            public string Gender { get; set; }

            [JsonProperty("birth")]
            public string Birth { get; set; }

            [JsonProperty("birth_day")]
            public string BirthDay { get; set; }

            [JsonProperty("birth_year")]
            public long BirthYear { get; set; }

            [JsonProperty("region")]
            public string Region { get; set; }

            [JsonProperty("address_id")]
            public long AddressId { get; set; }

            [JsonProperty("country_code")]
            public string CountryCode { get; set; }

            [JsonProperty("job")]
            public string Job { get; set; }

            [JsonProperty("job_id")]
            public long JobId { get; set; }

            [JsonProperty("total_follow_users")]
            public long TotalFollowUsers { get; set; }

            [JsonProperty("total_mypixiv_users")]
            public long TotalMypixivUsers { get; set; }

            [JsonProperty("total_illusts")]
            public long TotalIllusts { get; set; }

            [JsonProperty("total_manga")]
            public long TotalManga { get; set; }

            [JsonProperty("total_novels")]
            public long TotalNovels { get; set; }

            [JsonProperty("total_illust_bookmarks_public")]
            public long TotalIllustBookmarksPublic { get; set; }

            [JsonProperty("total_illust_series")]
            public long TotalIllustSeries { get; set; }

            [JsonProperty("total_novel_series")]
            public long TotalNovelSeries { get; set; }

            [JsonProperty("background_image_url")]
            public Uri BackgroundImageUrl { get; set; }

            [JsonProperty("twitter_account")]
            public string TwitterAccount { get; set; }

            [JsonProperty("twitter_url")]
            public Uri TwitterUrl { get; set; }

            [JsonProperty("is_premium")]
            public bool IsPremium { get; set; }

            [JsonProperty("is_using_custom_profile_image")]
            public bool IsUsingCustomProfileImage { get; set; }
        }

        public class ProfilePublicity
        {
            [JsonProperty("gender")]
            public string Gender { get; set; }

            [JsonProperty("region")]
            public string Region { get; set; }

            [JsonProperty("birth_day")]
            public string BirthDay { get; set; }

            [JsonProperty("birth_year")]
            public string BirthYear { get; set; }

            [JsonProperty("job")]
            public string Job { get; set; }

            [JsonProperty("pawoo")]
            public bool Pawoo { get; set; }
        }

        public class User
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("account")]
            public string Account { get; set; }

            [JsonProperty("profile_image_urls")]
            public ProfileImageUrls ProfileImageUrls { get; set; }

            [JsonProperty("comment")]
            public string Comment { get; set; }

            [JsonProperty("is_followed")]
            public bool IsFollowed { get; set; }
        }

        public class ProfileImageUrls
        {
            [JsonProperty("medium")]
            public string Medium { get; set; }
        }

        public class Workspace
        {
            [JsonProperty("pc")]
            public string Pc { get; set; }

            [JsonProperty("monitor")]
            public string Monitor { get; set; }

            [JsonProperty("tool")]
            public string Tool { get; set; }

            [JsonProperty("scanner")]
            public string Scanner { get; set; }

            [JsonProperty("tablet")]
            public string Tablet { get; set; }

            [JsonProperty("mouse")]
            public string Mouse { get; set; }

            [JsonProperty("printer")]
            public string Printer { get; set; }

            [JsonProperty("desktop")]
            public string Desktop { get; set; }

            [JsonProperty("music")]
            public string Music { get; set; }

            [JsonProperty("desk")]
            public string Desk { get; set; }

            [JsonProperty("chair")]
            public string Chair { get; set; }

            [JsonProperty("comment")]
            public string Comment { get; set; }
        }
    }
}