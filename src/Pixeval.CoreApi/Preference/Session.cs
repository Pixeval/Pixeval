#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/Session.cs
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

using System;
using System.Text.Json.Serialization;
using Pixeval.Utilities;

namespace Pixeval.CoreApi.Preference;

public record Session
{
    /// <summary>
    ///     User name
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Token expiration
    /// </summary>
    [JsonPropertyName("expireIn")]
    public DateTimeOffset ExpireIn { get; set; }

    /// <summary>
    ///     Current access token
    /// </summary>
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    /// <summary>
    ///     Current refresh token
    /// </summary>
    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }

    /// <summary>
    ///     Avatar
    /// </summary>
    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    ///     User id
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    ///     Account for login
    /// </summary>
    [JsonPropertyName("account")]
    public string? Account { get; set; }

    /// <summary>
    ///     Indicates whether current user is Pixiv Premium or not
    /// </summary>
    [JsonPropertyName("isPremium")]
    public bool IsPremium { get; set; }

    /// <summary>
    ///     WebAPI cookie
    /// </summary>
    [JsonPropertyName("cookie")]
    public string? Cookie { get; set; }

    [JsonPropertyName("cookieCreation")]
    public DateTimeOffset CookieCreation { get; set; }

    public override string? ToString()
    {
        return this.ToJson();
    }
}