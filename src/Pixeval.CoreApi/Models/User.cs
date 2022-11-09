#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/User.cs
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

namespace Pixeval.CoreApi.Models;

public record User
{
    [JsonPropertyName("user")]
    public UserInfo? UserInfo { get; set; }

    [JsonPropertyName("illusts")]
    public IEnumerable<Illustration>? Illusts { get; set; }

    [JsonPropertyName("is_muted")]
    public bool IsMuted { get; set; }
}

public class UserInfo
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("account")]
    public string? Account { get; set; }

    [JsonPropertyName("profile_image_urls")]
    public ProfileImageUrls? ProfileImageUrls { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("is_followed")]
    public bool IsFollowed { get; set; }
}