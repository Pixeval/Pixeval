#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/UserSpecifiedBookmarkTagResponse.cs
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

namespace Pixeval.CoreApi.Net.Response;

[Factory]
internal partial record UserSpecifiedBookmarkTagResponse
{
    [JsonPropertyName("error")]
    public required bool Error { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; } = "";

    [JsonPropertyName("body")]
    public required UserSpecifiedBookmarkTagBody ResponseBody { get; set; }
}

[Factory]
internal partial record UserSpecifiedBookmarkTagBody
{
    [JsonPropertyName("public")]
    public required UserSpecifiedBookmarkTag[] Public { get; set; } = [];

    [JsonPropertyName("private")]
    public required UserSpecifiedBookmarkTag[] Private { get; set; } = [];

    [JsonPropertyName("tooManyBookmark")]
    public required bool TooManyBookmark { get; set; }

    [JsonPropertyName("tooManyBookmarkTags")]
    public required bool TooManyBookmarkTags { get; set; }
}

[Factory]
internal partial record UserSpecifiedBookmarkTag
{
    [JsonPropertyName("tag")]
    public required string Name { get; set; } = "";

    [JsonPropertyName("cnt")]
    public required long Count { get; set; }
}
