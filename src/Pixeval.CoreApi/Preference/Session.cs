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

/// <summary>
/// 
/// </summary>
/// <param name="Name">User name</param>
/// <param name="ExpireIn">Token expiration</param>
/// <param name="AccessToken">Current access token</param>
/// <param name="RefreshToken">Current refresh token</param>
/// <param name="AvatarUrl">Avatar</param>
/// <param name="Id">User id</param>
/// <param name="Account">Account for login</param>
/// <param name="IsPremium">Indicates whether current user is Pixiv Premium or not</param>
public record Session(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("expireIn")] DateTimeOffset ExpireIn,
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("avatarUrl")] string AvatarUrl,
    [property: JsonPropertyName("id")]
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    long Id,
    [property: JsonPropertyName("account")] string Account,
    [property: JsonPropertyName("isPremium")] bool IsPremium)
{
    public override string? ToString() => this.ToJson();
}
