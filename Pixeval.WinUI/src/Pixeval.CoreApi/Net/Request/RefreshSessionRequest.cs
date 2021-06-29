using Refit;

namespace Pixeval.CoreApi.Net.Request
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CA1822
    internal class RefreshSessionRequest
    {
        [AliasAs("refresh_token")]
        public string? RefreshToken { get; }

        [AliasAs("grant_type")]
        public string GrantType => "refresh_token";

        [AliasAs("client_id")]
        public string ClientId => "MOBrBDS8blbauoSck0ZfDbtuzpyT";

        [AliasAs("client_secret")]
        public string ClientSecret => "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj";

        [AliasAs("include_policy")]
        public string IncludePolicy => "true";

        public RefreshSessionRequest(string? refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
#pragma warning restore CA1822
}