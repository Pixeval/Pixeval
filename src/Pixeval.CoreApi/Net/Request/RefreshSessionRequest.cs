#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2023 Pixeval.CoreApi/RefreshSessionRequest.cs
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

namespace Pixeval.CoreApi.Net.Request;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CA1822
public class RefreshSessionRequest(string? refreshToken)
{
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; } = refreshToken;

    [JsonPropertyName("grant_type")]
    public string GrantType => "refresh_token";

    [JsonPropertyName("client_id")]
    public string ClientId => "MOBrBDS8blbauoSck0ZfDbtuzpyT";

    [JsonPropertyName("client_secret")]
    public string ClientSecret => "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj";

    [JsonPropertyName("include_policy")]
    public string IncludePolicy => "true";
}
#pragma warning restore CA1822
