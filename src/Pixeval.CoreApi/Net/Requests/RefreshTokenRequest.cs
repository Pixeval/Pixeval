#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/RefreshTokenRequest.cs
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

using Refit;

namespace Pixeval.CoreApi.Net.Requests;

public record RefreshTokenRequest([property: AliasAs("refresh_token")] string? RefreshToken)
{
    [AliasAs("grant_type")]
    public string GrantType { get; } = "refresh_token";

    [AliasAs("client_id")]
    public string ClientId { get; } = "MOBrBDS8blbauoSck0ZfDbtuzpyT";

    [AliasAs("client_secret")]
    public string ClientSecret { get; } = "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj";

    [AliasAs("include_policy")]
    public string IncludePolicy { get; } = "true";
}