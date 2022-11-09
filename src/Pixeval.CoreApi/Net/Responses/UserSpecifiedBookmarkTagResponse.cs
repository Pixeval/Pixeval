#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/UserSpecifiedBookmarkTagResponse.cs
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

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Responses;

// ReSharper disable UnusedAutoPropertyAccessor.Global
internal class UserSpecifiedBookmarkTagResponse
{
    [JsonPropertyName("error")]
    public bool Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("body")]
    public Body? ResponseBody { get; set; }

    public class Body
    {
        [JsonPropertyName("public")]
        public IEnumerable<Tag>? Public { get; set; }

        [JsonPropertyName("private")]
        public IEnumerable<Tag>? Private { get; set; }

        [JsonPropertyName("tooManyBookmark")]
        public bool TooManyBookmark { get; set; }

        [JsonPropertyName("tooManyBookmarkTags")]
        public bool TooManyBookmarkTags { get; set; }
    }

    public class Tag
    {
        [JsonPropertyName("tag")]
        public string? Name { get; set; }

        [JsonPropertyName("cnt")]
        public long Count { get; set; }
    }
}