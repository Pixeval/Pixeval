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

using Newtonsoft.Json;

namespace Pixeval.Data.Web.Response
{
    public class WebApiUserDetailResponse
    {
        [JsonProperty("body")]
        public Body ResponseBody { get; set; }

        public class Body
        {
            [JsonProperty("user_details")]
            public UserDetails UserDetails { get; set; }
        }

        public class UserDetails
        {
            [JsonProperty("cover_image")]
            public CoverImage CoverImage { get; set; }
        }

        public class CoverImage
        {
            [JsonProperty("profile_cover_image")]
            public ProfileCoverImage ProfileCoverImage { get; set; }
        }

        public class ProfileCoverImage
        {
            [JsonProperty("720x360")]
            public string The720X360 { get; set; }
        }
    }
}