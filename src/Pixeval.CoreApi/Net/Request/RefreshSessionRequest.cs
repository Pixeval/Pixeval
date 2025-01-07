// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

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
