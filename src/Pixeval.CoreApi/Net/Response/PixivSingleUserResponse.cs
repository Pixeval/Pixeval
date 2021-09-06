#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/PixivSingleUserResponse.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal class PixivSingleUserResponse
    {
        [JsonPropertyName("user")]
        public User.Info? UserEntity { get; set; }

        [JsonPropertyName("profile")]
        public Profile? UserProfile { get; set; }

        [JsonPropertyName("profile_publicity")]
        public ProfilePublicity? UserProfilePublicity { get; set; }

        [JsonPropertyName("workspace")]
        public Workspace? UserWorkspace { get; set; }

        public class Profile
        {
            [JsonPropertyName("webpage")]
            public string? Webpage { get; set; }

            [JsonPropertyName("gender")]
            public string? Gender { get; set; }

            [JsonPropertyName("birth")]
            public string? Birth { get; set; }

            [JsonPropertyName("birth_day")]
            public string? BirthDay { get; set; }

            [JsonPropertyName("birth_year")]
            public long BirthYear { get; set; }

            [JsonPropertyName("region")]
            public string? Region { get; set; }

            [JsonPropertyName("address_id")]
            public long AddressId { get; set; }

            [JsonPropertyName("country_code")]
            public string? CountryCode { get; set; }

            [JsonPropertyName("job")]
            public string? Job { get; set; }

            [JsonPropertyName("job_id")]
            public long JobId { get; set; }

            [JsonPropertyName("total_follow_users")]
            public long TotalFollowUsers { get; set; }

            [JsonPropertyName("total_mypixiv_users")]
            public long TotalMyPixivUsers { get; set; }

            [JsonPropertyName("total_illusts")]
            public long TotalIllusts { get; set; }

            [JsonPropertyName("total_manga")]
            public long TotalManga { get; set; }

            [JsonPropertyName("total_novels")]
            public long TotalNovels { get; set; }

            [JsonPropertyName("total_illust_bookmarks_public")]
            public long TotalIllustBookmarksPublic { get; set; }

            [JsonPropertyName("total_illust_series")]
            public long TotalIllustSeries { get; set; }

            [JsonPropertyName("total_novel_series")]
            public long TotalNovelSeries { get; set; }

            [JsonPropertyName("background_image_url")]
            public string? BackgroundImageUrl { get; set; }

            [JsonPropertyName("twitter_account")]
            public string? TwitterAccount { get; set; }

            [JsonPropertyName("twitter_url")]
            public string? TwitterUrl { get; set; }

            [JsonPropertyName("is_premium")]
            public bool IsPremium { get; set; }

            [JsonPropertyName("is_using_custom_profile_image")]
            public bool IsUsingCustomProfileImage { get; set; }
        }

        public class ProfilePublicity
        {
            [JsonPropertyName("gender")]
            public string? Gender { get; set; }

            [JsonPropertyName("region")]
            public string? Region { get; set; }

            [JsonPropertyName("birth_day")]
            public string? BirthDay { get; set; }

            [JsonPropertyName("birth_year")]
            public string? BirthYear { get; set; }

            [JsonPropertyName("job")]
            public string? Job { get; set; }

            [JsonPropertyName("pawoo")]
            public bool? Pawoo { get; set; }
        }

        public class ProfileImageUrls
        {
            [JsonPropertyName("medium")]
            public string? Medium { get; set; }
        }

        public class Workspace
        {
            [JsonPropertyName("pc")]
            public string? Pc { get; set; }

            [JsonPropertyName("monitor")]
            public string? Monitor { get; set; }

            [JsonPropertyName("tool")]
            public string? Tool { get; set; }

            [JsonPropertyName("scanner")]
            public string? Scanner { get; set; }

            [JsonPropertyName("tablet")]
            public string? Tablet { get; set; }

            [JsonPropertyName("mouse")]
            public string? Mouse { get; set; }

            [JsonPropertyName("printer")]
            public string? Printer { get; set; }

            [JsonPropertyName("desktop")]
            public string? Desktop { get; set; }

            [JsonPropertyName("music")]
            public string? Music { get; set; }

            [JsonPropertyName("desk")]
            public string? Desk { get; set; }

            [JsonPropertyName("chair")]
            public string? Chair { get; set; }

            [JsonPropertyName("comment")]
            public string? Comment { get; set; }
        }
    }
}