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
    public class UploadTagResponse
    {
        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("body")]
        public Body[] ResponseBody { get; set; }

        public class Body
        {
            [JsonProperty("tag")]
            public string Tag { get; set; }

            [JsonProperty("tag_translation")]
            public string TagTranslation { get; set; }

            // 假名
            [JsonProperty("tag_yomigana")]
            public string TagYomigana { get; set; }

            [JsonProperty("cnt")]
            public int Count { get; set; }
        }
    }
}